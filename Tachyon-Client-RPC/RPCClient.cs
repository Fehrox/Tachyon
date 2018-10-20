using TachyonClientIO;
using TachyonClientCommon;
using System;
using TachyonCommon;
using TachyonCommon.Hash;

namespace TachyonClientRPC 
{
    public class RPCClient 
    {

        IClient _client;
        Stub _stub;
        Asks _ask;
        AskHasher _hasher;
        
        public RecievedEvent OnRecieved {
            get { return _client.OnRecieved; }
            set { _client.OnRecieved = value; }
        }

        public ConnectionEvent OnDisconnected {
            get { return _client.OnDisconnected; }
            set { _client.OnDisconnected = value; }
        }

        public ConnectionEvent OnConnected {
            get { return _client.OnConnected; }
            set { _client.OnConnected = value; }
        }

        public ConnectionEvent OnFailedToConnect {
            get { return _client.OnFailedToConnect; }
            set { _client.OnFailedToConnect = value; }
        }

        public RPCClient(IClient client, ISerializer serializer) {
            _client = client;

            _stub = new Stub(serializer);
            _ask = new Asks(_stub, serializer);
            _hasher = new AskHasher();
        }

        public void Connect(string host, int port) {
            _client.Connect(host, port);
            OnRecieved += Recieved;
        }

        void Recieved(byte[] data) {
            if (_ask.IsAskPacket(data)) {
                _ask.Replied(data);
            } else
                _stub.Invoke(data);
        }
        
        public void On<T>(Action<T> clientMethod) {
            var methodHash = _hasher.HashMethod(clientMethod.Method.Name);
            _stub.Register(methodHash, clientMethod);
        }

        public void Send(string method, params object[] args) {
            var methodHash = _hasher.HashMethod(method);
            var sendData = _stub.PackSend(methodHash, args);
            _client.Send(sendData);
        }

        public void Ask<T>(string method, Action<T> reply, params object[] args) {
            var methodHash = _hasher.HashMethod(method);
            var sendData = _stub.PackSend(methodHash, args);
            sendData = _ask.PackAsk(sendData, reply);
            _client.Send(sendData);
        }

    }
}
