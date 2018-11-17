using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TachyonServerIO 
{

    public class Connection : IConnection 
    {

        bool _connected = true;
        AutoResetEvent _recieveLock = new AutoResetEvent(false);

        public RecievedEvent OnRecieved { get; set; }
        public ConnectionEvent OnDisconnected { get; set; }

        TcpClient _client;
        Queue<string> _sendQueue = new Queue<string>();

        public Connection(TcpClient client) {
            _client = client;
        }

        public void Start() {
            var stream = _client.GetStream();
            SendConnectAck(stream);
            new Thread(() => Read(stream)).Start();
        }

        void SendConnectAck(NetworkStream stream) {
            stream.Write(new byte[] { 0x0 }, 0, 1);
        }

        void Read(object streamObj) {
            var stream = streamObj as NetworkStream;
            while (_connected) {
                if (stream.CanRead && stream.DataAvailable) {
                    try {
                        int incomingBytes = stream.ReadByte();
                        byte[] buffer = new byte[incomingBytes];
                        var message = stream.BeginRead(
                            buffer, 0, incomingBytes,
                            new AsyncCallback(Recieved),
                            new Result {
                                Buffer = buffer,
                                Stream = stream
                            });
                        _recieveLock.WaitOne();
                    } catch (IOException) {
                        _connected = false;
                    }
                }
            }

            OnDisconnected?.Invoke();

        }

        void Recieved(IAsyncResult ar) {

            var recieved = (ar.AsyncState as Result);
            var stream = recieved.Stream;
            var buffer = recieved.Buffer;

            var bytesRead = stream.EndRead(ar);

            OnRecieved(buffer);
            _recieveLock.Set();

        }

        public void Send(byte[] data) {

            try {
                if (!_client.Connected) {
                    _connected = false;
                } else {
                    var stream = _client.GetStream();

                    Console.WriteLine("Sending " + data.Length + " bytes");
                    var headerBytes = new byte[] { (byte)data.Length };
                    stream.WriteByte((byte)data.Length);
                    Console.WriteLine("Bytes " + data.Length);

                    stream.WriteAsync(data);
                }
            } catch (IOException) {
                _connected = false;
            }
            
        }

        private class Result {
            public byte[] Buffer;
            public NetworkStream Stream;
        }

    }
}
