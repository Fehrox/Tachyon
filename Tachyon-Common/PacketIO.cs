using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace TachyonCommon {
    public class PacketIO {

        const int CHUNK_SIZE = 255;
        NetworkStream _stream;

        List<byte[]> _recieveBuffer = new List<byte[]>();
        AutoResetEvent _recieveLock = new AutoResetEvent(false);

        public PacketIO(NetworkStream stream) {
            _stream = stream;
        }

        public byte[] Recieve() {

            _recieveBuffer.Clear();
            int incomingBytes = _stream.ReadByte();
            bool smallMessage = incomingBytes < CHUNK_SIZE;
            bool lastChunk = false;

            do {

                var buffer = new byte[incomingBytes];
                _stream.BeginRead(
                    buffer, 0, incomingBytes,
                    new AsyncCallback(Recieved),
                    new Result {
                        Buffer = buffer,
                        Stream = _stream });
                _recieveLock.WaitOne();

                if (lastChunk | smallMessage) break;

                if (incomingBytes == CHUNK_SIZE)
                    incomingBytes = _stream.ReadByte();

                if(incomingBytes != CHUNK_SIZE)
                    lastChunk = true;

            } while (
                (incomingBytes == CHUNK_SIZE && 
                // Incoming bytes are written as 0 
                // when the message is eactly CHUNK_SIZE.
                incomingBytes != 0) | lastChunk );

            var recievedBytes = _recieveBuffer
                .SelectMany(chunk => chunk)
                .ToArray();
            return recievedBytes;
        }

        void Recieved(IAsyncResult ar) {

            var recieved = (ar.AsyncState as Result);
            var stream = recieved.Stream;
            var buffer = recieved.Buffer;

            var bytesRead = stream.EndRead(ar);
            _recieveBuffer.Add(buffer);

            _recieveLock.Set();

        }

        private class Result {
            public byte[] Buffer;
            public NetworkStream Stream;
        }

        public void Send(byte[] data) {
            for (int i = 0; i < data.Length; i += CHUNK_SIZE) {

                var writeSize = Math.Min(CHUNK_SIZE, data.Length - i);
                var chunk = Slice(data, i, writeSize);

                var lengthByte = BitConverter.GetBytes((short)chunk.Length)[0];
                _stream.WriteByte( lengthByte );

                _stream.BeginWrite(
                    chunk, 0, chunk.Length, 
                    new AsyncCallback(Sent), 
                    _stream );

                // Message is exactly of length CHUNK_SIZE, 
                // write 0 to signal end of packet.
                var nextChunkWouldOverflow = i + CHUNK_SIZE > data.Length;
                if (writeSize == CHUNK_SIZE && nextChunkWouldOverflow)
                    _stream.WriteByte(BitConverter.GetBytes((short)0)[0]);
            }
        }

        void Sent(IAsyncResult ar) {
            var stream = (ar.AsyncState as NetworkStream);
            stream.EndWrite(ar);
        }

        byte[] Slice(byte[] buffer, int index, int length) {
            byte[] slice = new byte[length];
            Array.Copy(buffer, index, slice, 0, length);
            return slice;
        }

    }
}
