using TachyonCommon.Hash;
using TachyonServerIO;
using System.Net.Sockets;
using TachyonCommon;
using System;
using System.IO;
using System.Text;

namespace TachyonServerRPC {
    public class ClientConnection {
        public readonly string Guid;

        private readonly IReplyProcessor _replyProcessor;
        private readonly Connection _connection;
        private readonly AskHasher _hasher;
        private readonly HostStub _stub;
        private readonly Asks _ask;

        public RecievedEvent OnRecieved {
            get => _connection.OnRecieved;
            set => _connection.OnRecieved = value;
        }

        public ConnectionEvent OnDisconnected {
            get => _connection.OnDisconnected;
            set => _connection.OnDisconnected = value;
        }

        public ClientConnection(
            TcpClient client,
            HostStub stub,
            AskHasher hasher,
            ISerializer serializer,
            IReplyProcessor replyProcessor
        ) {
            Guid = System.Guid.NewGuid().ToString();
            _connection = new Connection(client);
            _hasher = hasher;
            _stub = stub;
            _ask = new Asks(serializer);
            _replyProcessor = replyProcessor;
        }

        public void Start() {
            _connection.Start();
            _connection.OnRecieved += (data) => {
                var requestHash = new[] {data[0], data[1]};

                var isAsk = _ask.IsReplyPacket(data);
                const short METHOD_HEADER = 2, CALLBACK_HEADER = 4;
                var method = _stub.GetMethod(
                    data,
                    isAsk
                        ? (short) (METHOD_HEADER + CALLBACK_HEADER)
                        : METHOD_HEADER,
                    Guid
                );

                if (method == null)
                    throw new InvalidDataException("Received packed for unregistered method.");

                _replyProcessor.ProcessReply(() => {
                    var replyObject = method.Invoke();
                    if (isAsk) {
                        Array.Copy(requestHash, data, requestHash.Length);
                        data = _ask.PackReply(data, replyObject);
                        _connection.Send(data);
                    }
                });
            };
        }

        public void Send(string method, params object[] args) {
            var methodHash = _hasher.HashString(method);
            var data = _stub.PackSend(methodHash, args);
            _connection.Send(data);
        }
    }
}