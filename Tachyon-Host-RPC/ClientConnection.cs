using TachyonClientCommon;
using TachyonCommon.Hash;
using TachyonServerIO;
using System.Net.Sockets;
using TachyonCommon;
using System;
using System.Text;

namespace TachyonServerRPC
{
    public class ClientConnection
    {
        public readonly string Guid;

        private Connection _connection;
        private AskHasher _hasher;
        private HostStub _stub;
        private Asks _ask;

        public RecievedEvent OnRecieved
        {
            get => _connection.OnRecieved;
            set => _connection.OnRecieved = value;
        }

        public ConnectionEvent OnDisconnected
        {
            get => _connection.OnDisconnected;
            set => _connection.OnDisconnected = value;
        }

        public ClientConnection(
            TcpClient client,
            HostStub stub,
            AskHasher hasher,
            ISerializer serializer
        )
        {
            Guid = System.Guid.NewGuid().ToString();
            _connection = new Connection(client);
            _hasher = hasher;
            _stub = stub;
            _ask = new Asks(_stub, serializer);
        }

        public void Start()
        {
            _connection.Start();
            _connection.OnRecieved += (data) =>
            {
                var requestHash = new[] {data[0], data[1]};

                var isAsk = _ask.IsReplyPacket(data);
                const short METHOD_HEADER = 2, CALLBACK_HEADER = 4;
                var method = _stub.GetMethod(
                    data,
                    isAsk 
                        ? (short)(METHOD_HEADER + CALLBACK_HEADER) 
                        : METHOD_HEADER,
                    Guid
                );
                
                if (method == null) {
                    Console.WriteLine("Recieved packet for unregistered method.");
                    return;
                }

                var replyObject = method.Invoke();
                if (isAsk) {
                    Array.Copy(requestHash, data, requestHash.Length);
                    data = _ask.PackReply(data, replyObject);
                    _connection.Send(data);
                }
            };
        }

        public void Send(string method, params object[] args)
        {
            var methodHash = _hasher.HashString(method);
            var data = _stub.PackSend(methodHash, args);
            _connection.Send(data);
        }
    }
}