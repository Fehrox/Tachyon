﻿using TachyonClientCommon;
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

        public readonly string Guid;

        Connection _connection;
        AskHasher _hasher;
        ServerStub _stub;
        Asks _ask;

        public RecievedEvent OnRecieved {
            get { return _connection.OnRecieved; }
            set { _connection.OnRecieved = value; }
        }

        public ConnectionEvent OnDisconnected {
            get { return _connection.OnDisconnected; }
            set { _connection.OnDisconnected = value; }
        }

        public ConnectedClient(
            TcpClient client, 
            ServerStub stub, 
            AskHasher hasher, 
            ISerializer serializer
        ) {
            Guid = System.Guid.NewGuid().ToString();
            _connection = new Connection(client);
            _hasher = hasher;
            _stub = stub;
            _ask = new Asks(_stub, serializer);
        }

        public void Start() {
            _connection.Start();
            _connection.OnRecieved += (data) => {

                Console.WriteLine( Guid + " " + Encoding.ASCII.GetString(data));

                var isAsk = _ask.IsAskPacket(data);
                const short METHOD_HEADER = 2, CALLBACK_HEADER = 4;
                var method = _stub.GetMethod(data, 
                    isAsk ? CALLBACK_HEADER : METHOD_HEADER, 
                    Guid );
                if (method == null) {
                    Console.WriteLine("Recieved packet for unregistered method.");
                    return;
                }

                var replyData = method.Invoke();
                if (isAsk) {
                    data = _ask.PackReply(data, replyData);
                    _connection.Send(data);
                }

            };
        }  

        public void Send(string method, params object[] args) {
            var methodHash = _hasher.HashMethod(method);
            var data = _stub.PackSend(methodHash, args);
            _connection.Send(data);
        }

    }
}
