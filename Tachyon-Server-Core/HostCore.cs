using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TachyonServerRPC;

namespace TachyonServerCore {

    public class HostCore {

        List<ConnectedClient> _clients = new List<ConnectedClient>();
        readonly EndpointMap _endPoints;

        public HostCore(EndpointMap endPoints) {
            _endPoints = endPoints;
        }

        public HostCore Start() {
            AcceptClients(Listen());
            return this;
        }

        public void Broadcast(string method, params object[] args) {
            for(int i = 0; i < _clients.Count; i++) 
                _clients[i].Send(method, args);
        }

        private static TcpListener Listen() {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 13);
            tcpListener.Start();
            return tcpListener;
        }

        private void AcceptClients(TcpListener tcpListener) {
            new Thread(() => {
                while (true) {

                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    var stub = _endPoints.Stub;
                    var hasher = _endPoints.Hasher;
                    var connection = new ConnectedClient(tcpClient, stub, hasher, _endPoints.Serializer);
                    _endPoints.RegisterConnection(connection);
                    connection.Start();
                    
                    connection.OnDisconnected += () => {
                        Console.WriteLine("Client Disconnected");
                        _clients.Remove(connection);
                    };

                    _clients.Add(connection);                    
                    Console.WriteLine("Client connected");
                    Thread.Sleep(10);
                }
            }).Start();
        }

    }

}
