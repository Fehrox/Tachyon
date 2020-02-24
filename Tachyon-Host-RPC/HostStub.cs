using System;
using TachyonClientCommon;
using TachyonCommon;
using TachyonCommon.Hash;

namespace TachyonServerRPC
{
    public class HostStub : Stub
    {
        public HostStub(ISerializer serializer)
            : base(serializer)
        {
        }

        public void Register<T>(
            short methodHash,
            Action<T> method,
            string guid
        )
        {
            methodHash = DistinguishClientHash(methodHash, guid);
            Register<T>(methodHash, method);
        }

        public void Register<I, O>(
            short methodHash,
            Func<I, O> func,
            string guid
        )
        {
            methodHash = DistinguishClientHash(methodHash, guid);
            Register<I, O>(methodHash, func);
        }

        public InvocationDescriptor GetMethod(
            byte[] data,
            short argStartIndex,
            string guid 
        ) {
            var methodHash = BitConverter.ToInt16(new[] {data[0], data[1]}, 0);
            methodHash = DistinguishClientHash(methodHash, guid);
            var methodHashBytes = BitConverter.GetBytes(methodHash);
            data[0] = methodHashBytes[0];
            data[1] = methodHashBytes[1];
            return UnpackReceive(data, argStartIndex);
        }

        /// <summary>
        /// Ensures multiple ConnectedClients are able to 
        /// register their their callbacks without conflicting.
        /// </summary>
        /// <param name="methodHash">Unmodified method hash</param>
        /// <param name="guid">Client connection Guid.</param>
        /// <returns>Method hash unique to given client Guid.</returns>
        private short DistinguishClientHash(
            short methodHash,
            string guid
        )
        {
            var hasher = new Hasher();
            return (short) (methodHash + hasher.HashString(guid));
        }
    }
}