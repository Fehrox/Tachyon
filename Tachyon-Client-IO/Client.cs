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
        public event ConnectionEvent OnFailedToConnect;

        private NetworkStream _stream;
        private PacketIO _packetIO;

        public void Connect(string host, int port)
        {
            var client = new TcpClient();
            client.BeginConnect(host, port, new AsyncCallback(Connected), client);
        }

        private void Connected(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            try {
                client.EndConnect(ar);
            } catch (SocketException) {
                OnFailedToConnect?.Invoke();
                return;
            }

            if (client.Connected)
            {
                _stream = client.GetStream();
                _packetIO = new PacketIO(_stream);
                _connected = true;
                OnConnected?.Invoke();
                new Thread(() => { Recieve(client); }).Start();
            }
            else
            {
                OnFailedToConnect?.Invoke();
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

            //var headerBytes = new byte[] { (byte)data.Length };

            //_stream.Write(headerBytes, 0, 1);
            //_stream.BeginWrite(data, 0, data.Length,
            //    new AsyncCallback(Sent),
            //    _stream);
        }

        //void Sent(IAsyncResult ar) {
        //    var stream = (ar.AsyncState as NetworkStream);
        //    stream.EndWrite(ar);
        //}

        private void Recieve(TcpClient client)
        {
            var stream = client.GetStream();

            while (_connected)
                if (stream.CanRead)
                    try
                    {
                        var receivedBytes = _packetIO.Recieve();
                        if (receivedBytes.Length == 0) return;

                        OnRecieved?.Invoke(receivedBytes);
                        
                        //int incomingBytes = stream.ReadByte();
                        //byte[] buffer = new byte[incomingBytes];

                        //stream.BeginRead(
                        //    buffer, 0, incomingBytes,
                        //    new AsyncCallback(Recieved),
                        //    new ReadResult {
                        //        Buffer = buffer,
                        //        Stream = stream
                        //    });
                        //_wait.WaitOne();
                    }
                    catch (IOException)
                    {
                        _connected = false;
                    }

            OnDisconnected?.Invoke();
        }

        //private void Recieved(IAsyncResult ar) {

        //    var recieved = (ar.AsyncState as ReadResult);
        //    var stream = recieved.Stream;
        //    var buffer = recieved.Buffer;

        //    try {
        //        var bytesRead = stream.EndRead(ar);
        //        _wait.Set();

        //        if(bytesRead > 0) OnRecieved?.Invoke(buffer);
        //    } catch (IOException) {
        //        _connected = false;
        //    }
        //}

        // private class ReadResult
        // {
        //     public byte[] Buffer;
        //     public NetworkStream Stream;
        // }
    }
}