using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using TachyonCommon;

namespace TachyonClientIO
{
    public class Client : IClient
    {
        private bool _connected = false;

        public event RecievedEvent OnRecieved;
        public event ConnectionEvent OnDisconnected;
        public event ConnectionEvent OnConnected;
        public event FailedConnectionEvent OnFailedToConnect;

        private NetworkStream _stream;
        private PacketIO _packetIO;

        public void Connect(string host, int port)
        {
            var client = new TcpClient();
            client.BeginConnect(host, port, Connected, client);
        }

        private void Connected(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            try {
                client.EndConnect(ar);
            } catch (SocketException ex) {
                OnFailedToConnect?.Invoke(new ConnectionFailedException(ex));
                return;
            }

            if (client.Connected)
            {
                _stream = client.GetStream();
                _packetIO = new PacketIO(_stream);
                _connected = true;
                OnConnected?.Invoke();
                new Thread(() => { Recieve(client); }).Start();
            } else {
                var ex = new ConnectionFailedException(
                    "Unable to connect to host.");
                OnFailedToConnect?.Invoke(ex);
            }
        }

        public void Send(byte[] data)
        {
            if (!_connected) return;

            try {
                _packetIO.Send(data);
            } catch(IOException) {
                _connected = false;
                OnDisconnected?.Invoke();
            }

        }


        private void Recieve(TcpClient client)
        {
            var stream = client.GetStream();

            while (_connected)
                if (stream.CanRead)
                    try {
                        var receivedBytes = _packetIO.Recieve();
                        if (receivedBytes.Length == 0) return;

                        OnRecieved?.Invoke(receivedBytes);
                        
                    } catch (IOException) {
                        _connected = false;
                    }

            OnDisconnected?.Invoke();
        }
        
    }
}