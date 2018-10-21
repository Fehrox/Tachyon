using TachyonClientCommon;
using TachyonCommon.Hash;
using TachyonServerIO;
using System.Net.Sockets;
using TachyonCommon;
using System;
using System.Text;

namespace TachyonServerRPC 
{
    public class ConnectedClient
    {
        
        AskHasher _hasher;
        Server _server;
        Stub _stub;
        Asks _ask;

        public RecievedEvent OnRecieved {
            get { return _server.OnRecieved; }
            set { _server.OnRecieved = value; }
        }

        public ConnectionEvent OnDisconnected {
            get { return _server.OnDisconnected; }
            set { _server.OnDisconnected = value; }
        }

        public ConnectedClient(TcpClient client, Stub stub, AskHasher hasher, ISerializer serializer) {
            _hasher = hasher;
            _stub = stub;
            _server = new Server(client);
            _ask = new Asks(_stub, serializer);
        }

        public void Start() {
            _server.Start();
            _server.OnRecieved += (data) => {

                Console.WriteLine(Encoding.ASCII.GetString(data));

                var isAsk = _ask.IsAskPacket(data);
                const short METHOD_HEADER = 2, CALLBACK_HEADER = 4;
                var method = _stub.GetMethod(data, isAsk ? CALLBACK_HEADER : METHOD_HEADER);
                if (method == null) {
                    // TODO: Find what causes this to become null.
                    Console.WriteLine("Recieved invalid method data");
                    return;
                }

                var replyData = method.Invoke();
                if (isAsk) {
                    data = _ask.PackReply(data, replyData);
                    _server.Send(data);
                }

            };
        }  

        public void Send(string method, params object[] args) {
            var methodHash = _hasher.HashMethod(method);
            var data = _stub.PackSend(methodHash, args);
            _server.Send(data);
        }

    }
}
