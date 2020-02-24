using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TachyonServerRPC;

namespace TachyonServerCore {
    public class HostCore {
        
        public event Action OnStarted;
        public event Action<ClientConnection> OnDisconnected;
        public event Action<ClientConnection> OnConnected;

        private List<ClientConnection> _clients = new List<ClientConnection>();
        public readonly EndPointMap _endPoints;

        public HostCore(EndPointMap endPoints) {
            _endPoints = endPoints;
        }

        public HostCore Start() {
            AcceptClients(Listen());
            return this;
        }

        public void Broadcast(string method, params object[] args) {
            for (var i = 0; i < _clients.Count; i++)
                _clients[i].Send(method, args);
        }

        private static TcpListener Listen() {
            var tcpListener = new TcpListener(IPAddress.Any, 13);
            tcpListener.Start();
            return tcpListener;
        }

        private void AcceptClients(TcpListener tcpListener) {
            new Thread(() => {
                while (true) {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    var stub = _endPoints.Stub;
                    var hasher = _endPoints.Hasher;
                    var connection = new ClientConnection(
                        tcpClient, stub, hasher, _endPoints.Serializer);
                    _endPoints.RegisterConnection(connection);
                    connection.Start();

                    connection.OnDisconnected += () => {
                        OnDisconnected?.Invoke(connection);
                        _clients.Remove(connection);
                    };

                    _clients.Add(connection);
                    OnConnected?.Invoke(connection);

                    Thread.Sleep(10);
                }
            }).Start();

            OnStarted?.Invoke();
        }
    }
}