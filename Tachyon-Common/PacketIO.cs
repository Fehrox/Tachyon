using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System;

namespace TachyonCommon {
    public class PacketIO {

        const int CHUNK_SIZE = 255;
        const int ONE_BYTE = 1;

        NetworkStream _stream;

        public PacketIO(NetworkStream stream) {
            _stream = stream;
        }

        public byte[] Recieve() {

            List<byte[]> recieveBuffer = new List<byte[]>();

            short incomingBytes = ReadIncomingBytes();
            bool smallMessage = incomingBytes < CHUNK_SIZE;
            bool lastChunk = false;

            do {

                var buffer = new byte[incomingBytes];
                _stream.Read(buffer, 0, incomingBytes);
                recieveBuffer.Add(buffer);

                if (lastChunk | smallMessage) break;

                if (incomingBytes == CHUNK_SIZE)
                    incomingBytes = ReadIncomingBytes();

                if (incomingBytes != CHUNK_SIZE)
                    lastChunk = true;

            } while (
                (incomingBytes == CHUNK_SIZE &&
                // Incoming bytes are written as 0 
                // when the message is eactly CHUNK_SIZE.
                incomingBytes != 0) | lastChunk);

            var recievedBytes = recieveBuffer
                .SelectMany(chunk => chunk)
                .ToArray();
            return recievedBytes;
        }

        private short ReadIncomingBytes() {
            var lengthPacket = new byte[ONE_BYTE];
            _stream.Read(lengthPacket, 0, ONE_BYTE);
            return lengthPacket[0];
        }

        public void Send(byte[] data) {
            for (int i = 0; i < data.Length; i += CHUNK_SIZE) {

                var writeSize = Math.Min(CHUNK_SIZE, data.Length - i);
                var chunk = Slice(data, i, writeSize);

                var lengthPacket = BitConverter.GetBytes((short)chunk.Length)[0];
                var combinedPacket = new[] { lengthPacket }.Concat(chunk).ToArray();
                _stream.Write(combinedPacket, 0, ONE_BYTE + chunk.Length);

                // Message is exactly of length CHUNK_SIZE, 
                // write 0 to signal end of packet.
                if (writeSize == CHUNK_SIZE && i + CHUNK_SIZE >= data.Length) {
                    _stream.WriteByte(new byte());
                    break;
                }
            }
        }

        byte[] Slice(byte[] buffer, int index, int length) {
            byte[] slice = new byte[length];
            Array.Copy(buffer, index, slice, 0, length);
            return slice;
        }

    }
}
