using TachyonClientIO;
using TachyonClientCommon;
using System;
using TachyonCommon;
using TachyonCommon.Hash;

namespace TachyonClientRPC
{
    public class ClientRpc
    {
        private IClient _client;
        private Stub _stub;
        private Asks _ask;
        private AskHasher _hasher;

        public event RecievedEvent OnRecieved;

        public ConnectionEvent OnDisconnected;

        public ConnectionEvent OnConnected;

        public ConnectionEvent OnFailedToConnect;

        public ClientRpc(IClient client, ISerializer serializer)
        {
            _client = client;

            _stub = new Stub(serializer);
            _ask = new Asks(_stub, serializer);
            _hasher = new AskHasher();
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port);

            _client.OnConnected += () => OnConnected?.Invoke();
            _client.OnDisconnected += () => OnDisconnected?.Invoke();
            _client.OnRecieved += (r) => OnRecieved?.Invoke(r);
            _client.OnFailedToConnect += () => OnFailedToConnect?.Invoke();
            
            OnRecieved += Recieved;
        }

        private void Recieved(byte[] data)
        {
            // TODO: Find what causes this to become null.
            if (data == null) return;

            if (_ask.IsReplyPacket(data))
                _ask.Replied(data);
            else
                _stub.Invoke(data);
        }

        public void On<T>(Action<T> clientMethod)
        {
            var methodHash = _hasher.HashString(clientMethod.Method.Name);
            _stub.Register(methodHash, clientMethod);
        }

        public void Send(string method, params object[] args)
        {
            var methodHash = _hasher.HashString(method);
            var sendData = _stub.PackSend(methodHash, args);
            _client.Send(sendData);
        }

        public void Ask<T>(string method, Action<T> reply, params object[] args)
        {
            var methodHash = _hasher.HashString(method);
            var sendData = _stub.PackSend(methodHash, args);
            sendData = _ask.PackAsk(sendData, reply);
            _client.Send(sendData);
        }
    }
}