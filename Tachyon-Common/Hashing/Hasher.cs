using System;
using System.Security.Cryptography;
using System.Text;

namespace TachyonCommon.Hash
{
    public class Hasher : IHasher
    {
        public virtual short HashMethod(string method)
        {
            var methodBytes = Encoding.UTF8.GetBytes(method);

            var algorithm = MD5.Create();
            var hash = algorithm.ComputeHash(methodBytes);
            var shortHash = new byte[2] {hash[0], hash[1]};

            var methodHash = BitConverter.ToInt16(shortHash, 0);

            return methodHash;
        }
    }
}