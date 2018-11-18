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
        PacketIO _packetIO;

        //const int CHUNK_SIZE = 254;
        bool _connected = true;
        //AutoResetEvent _recieveLock = new AutoResetEvent(false);
        //List<byte[]> _recieveBuffer = new List<byte[]>();

        public RecievedEvent OnRecieved { get; set; }
        public ConnectionEvent OnDisconnected { get; set; }

        TcpClient _client;
        //Queue<string> _sendQueue = new Queue<string>();

        public Connection(TcpClient client) {
            _packetIO = new PacketIO(client.GetStream());
            _client = client;
        }

        public void Start() {
            var stream = _client.GetStream();
            //SendConnectAck(stream);
            new Thread(() => Read(stream)).Start();
        }

        //void SendConnectAck(NetworkStream stream) {
        //    stream.Write(new byte[] { 0x0 }, 0, 1);
        //}

        void Read(object streamObj) {
            var stream = streamObj as NetworkStream;
            while (_connected) {
                if (stream.CanRead && stream.DataAvailable) {
                    try {

                        var recievedBytes = _packetIO.Recieve();
                        OnRecieved(recievedBytes);
                        //_recieveBuffer.Clear();
                        //int incomingBytes = stream.ReadByte();
                        //do {

                        //    byte[] buffer = new byte[incomingBytes];
                        //    var message = stream.BeginRead(
                        //        buffer, 0, incomingBytes,
                        //        new AsyncCallback(Recieved),
                        //        new Result {
                        //            Buffer = buffer,
                        //            Stream = stream
                        //        });
                        //    _recieveLock.WaitOne();

                        //    incomingBytes = stream.ReadByte();

                        //} while (incomingBytes == CHUNK_SIZE);

                        //var recievedBytes = _recieveBuffer
                        //    .SelectMany(chunk => chunk)
                        //    .ToArray();
                        //OnRecieved(recievedBytes);

                    } catch (IOException) {
                        _connected = false;
                    }
                }
            }

            OnDisconnected?.Invoke();

        }

        //void Recieved(IAsyncResult ar) {

        //    var recieved = (ar.AsyncState as Result);
        //    var stream = recieved.Stream;
        //    var buffer = recieved.Buffer;

        //    var bytesRead = stream.EndRead(ar);
        //    _recieveBuffer.Add(buffer);

        //    _recieveLock.Set();

        //}

        public void Send(byte[] data) {

            try {
                if (!_client.Connected) {
                    _connected = false;
                } else {
                    var stream = _client.GetStream();

                    _packetIO.Send(data);
                    //Console.WriteLine("Sending " + data.Length + " bytes");

                    //for (int i = 0; i < data.Length; i += CHUNK_SIZE) {

                    //    // If the size of a chunk is anything other than 
                    //    // CHUNK_SIZE the reciever will know it's the end 
                    //    // of the message.
                    //    var currentChunkSize = data.Length - i;
                    //    var writeSize = Math.Min(CHUNK_SIZE, currentChunkSize);
                    //    if (currentChunkSize == CHUNK_SIZE + 1)
                    //        writeSize = currentChunkSize;
                    //    var chunk = Slice(data, i, writeSize);

                    //    Console.WriteLine("Sending CHUNK of " + chunk.Length + " bytes");
                    //    var lengthByte = BitConverter.GetBytes((short)chunk.Length)[0];
                    //    stream.WriteByte(lengthByte);
                    //    Console.WriteLine("Bytes " + chunk.Length);

                    //    stream.WriteAsync(chunk);

                    //}
                }
            } catch (IOException) {
                _connected = false;
            }
            
        }

        byte[] Slice(byte[] buffer, int index, int length) {
            byte[] slice = new byte[length];
            Array.Copy(buffer, index, slice, 0, length);
            return slice;
        }

        //private class Result {
        //    public byte[] Buffer;
        //    public NetworkStream Stream;
        //}

    }
}
