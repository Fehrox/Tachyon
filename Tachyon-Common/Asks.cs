using System;
using System.Collections.Generic;
using System.Linq;
using TachyonClientCommon;
using TachyonCommon.Hash;

namespace TachyonCommon
{
    public class Asks
    {
        private const int ASK_BIT_INDEX = 0;
        private SortedList<short, Stub.RemoteMethod> _pendingResponses = new SortedList<short, Stub.RemoteMethod>();
        private List<short> _pendingRequests = new List<short>();
        private ISerializer _serializer;

        public Asks(Stub stub, ISerializer serializer)
        {
            _serializer = serializer;
        }

        public bool IsReplyPacket(byte[] bytes)
        {
            return ConvertByteToBitAddres(bytes[0], ASK_BIT_INDEX);
        }

        public void Replied(byte[] response)
        {
            short methodHashSize = sizeof(short),
                callbackIdSize = sizeof(short);

            var callingMethodHash = BitConverter.ToInt16(response, 0);
            var isRegistedRequest = _pendingRequests.Contains(callingMethodHash);
            if (isRegistedRequest)
            {
                var callbackId = BitConverter.ToInt16(response, methodHashSize);
                var request = _pendingResponses[callbackId];

                var argData = response.Skip(methodHashSize + callbackIdSize);
                var callbackArgs = request.Method.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                var parameters = new List<object>();
                if (callbackArgs.Length > 1) {
                    throw new NotImplementedException("Handle multiple params.");
                } else {
                    var callbackArg = callbackArgs.First();
                    var arg = _serializer.DeserializeObject(
                        argData.ToArray(), 
                        callbackArg );
                    parameters.Add(arg);
                }
                
                request.Method.Invoke(request.Target, parameters.ToArray() );
                _pendingRequests.Remove(callingMethodHash);
            }
            else
            {
                Console.WriteLine("Recieved response to " +
                                  "unregestered request " + callingMethodHash);
            }
        }

        public byte[] PackReply(byte[] data, object reply)
        {
            //var replyJson = _serializer.SerializeObject(reply);
            //var replyArgData = Encoding.ASCII.GetBytes(replyJson);
            var replyArgData = _serializer.SerializeObject(reply);

            var methodHashSize = sizeof(short);
            var callbackIdSize = sizeof(short);
            var headerBytes = data.Take(methodHashSize + callbackIdSize);

            var replyData = headerBytes.Concat(replyArgData);
            return replyData.ToArray();
        }

        public byte[] PackAsk<T>(byte[] data, Action<T> reply)
        {
            var methodHash = BitConverter.ToInt16(data, 0);
            methodHash = AskHasher.SetAskBit(methodHash, true);
            var methodHashBytes = BitConverter.GetBytes(methodHash);
            _pendingRequests.Add(methodHash);

            var callbackId = _pendingResponses.Any() ? (_pendingResponses.Last().Key + 1) % short.MaxValue : 0;
            var callbackIdBytes = BitConverter.GetBytes(callbackId);
            var callback = new Stub.RemoteMethod(reply.Method, reply.Target);
            _pendingResponses.Add((short) callbackId, callback);

            var dataBody = data.Skip(methodHashBytes.Length).ToArray();

            var dataLen = methodHashBytes.Length + callbackIdBytes.Length + dataBody.Length;
            var packedData = new byte[dataLen];
            methodHashBytes.CopyTo(packedData, 0);
            callbackIdBytes.CopyTo(packedData, methodHashBytes.Length);
            dataBody.CopyTo(packedData, methodHashBytes.Length + callbackIdBytes.Length);

            return packedData;
        }

        //public void ClearAskBit(ref byte[] sendData) {
        //    var askFlag = BitConverter.ToInt16(sendData, 0);
        //    var askHeader = SetAskFlag(askFlag, false);
        //    var askHeaderBytes = BitConverter.GetBytes(askHeader);
        //    askHeaderBytes.CopyTo(sendData, 0);
        //}


        private bool ConvertByteToBitAddres(byte byteToConvert, int bitToReturn)
        {
            var mask = 1 << bitToReturn;
            return (byteToConvert & mask) == mask;
        }
    }
}