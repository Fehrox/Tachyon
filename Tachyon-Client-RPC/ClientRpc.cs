using TachyonClientIO;
using TachyonClientCommon;
using System;
using TachyonCommon;
using TachyonCommon.Hash;

namespace TachyonClientRPC
{
    public class Client
    {
        private IClient _client;
        private Stub _stub;
        private Asks _ask;
        private AskHasher _hasher;

        public RecievedEvent OnRecieved
        {
            get => _client.OnRecieved;
            set => _client.OnRecieved = value;
        }

        public ConnectionEvent OnDisconnected
        {
            get => _client.OnDisconnected;
            set => _client.OnDisconnected = value;
        }

        public ConnectionEvent OnConnected
        {
            get => _client.OnConnected;
            set => _client.OnConnected = value;
        }

        public ConnectionEvent OnFailedToConnect
        {
            get => _client.OnFailedToConnect;
            set => _client.OnFailedToConnect = value;
        }

        public Client(IClient client, ISerializer serializer)
        {
            _client = client;

            _stub = new Stub(serializer);
            _ask = new Asks(_stub, serializer);
            _hasher = new AskHasher();
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port);
            OnRecieved += Recieved;
        }

        private void Recieved(byte[] data)
        {
            // TODO: Find what causes this to become null.
            if (data == null) return;

            if (_ask.IsAskPacket(data))
                _ask.Replied(data);
            else
                _stub.Invoke(data);
        }

        public void On<T>(Action<T> clientMethod)
        {
            var methodHash = _hasher.HashMethod(clientMethod.Method.Name);
            _stub.Register(methodHash, clientMethod);
        }

        public void Send(string method, params object[] args)
        {
            var methodHash = _hasher.HashMethod(method);
            var sendData = _stub.PackSend(methodHash, args);
            _client.Send(sendData);
        }

        public void Ask<T>(string method, Action<T> reply, params object[] args)
        {
            var methodHash = _hasher.HashMethod(method);
            var sendData = _stub.PackSend(methodHash, args);
            sendData = _ask.PackAsk(sendData, reply);
            _client.Send(sendData);
        }
    }
}