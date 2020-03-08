using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using TachyonCommon;

namespace TachyonServerIO
{
    public class Connection : IConnection
    {
        private PacketIO _packetIO;
        private bool _connected = true;

        public RecievedEvent OnRecieved { get; set; }
        public ConnectionEvent OnDisconnected { get; set; }

        private TcpClient _client;

        public Connection(TcpClient client)
        {
            _packetIO = new PacketIO(client.GetStream());
            _client = client;
        }

        public void Start()
        {
            var stream = _client.GetStream();
            new Thread(() => Read(stream)).Start();
        }


        private void Read(object streamObj)
        {
            var stream = streamObj as NetworkStream;
            while (_connected)
                if (stream.CanRead && stream.DataAvailable)
                    try
                    {
                        var recievedBytes = _packetIO.Recieve();
                        OnRecieved(recievedBytes);
                    }
                    catch (IOException)
                    {
                        _connected = false;
                    }

            OnDisconnected?.Invoke();
        }

        public void Send(byte[] data)
        {
            try
            {
                if (!_client.Connected)
                {
                    _connected = false;
                }
                else
                {
                    var stream = _client.GetStream();

                    _packetIO.Send(data);

                }
            }
            catch (IOException)
            {
                _connected = false;
            }
        }

        private byte[] Slice(byte[] buffer, int index, int length)
        {
            var slice = new byte[length];
            Array.Copy(buffer, index, slice, 0, length);
            return slice;
        }
    }
}