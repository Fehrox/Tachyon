using System;
using System.Collections.Generic;
using System.Text;

namespace TachyonCommon.Hash
{
    public class AskHasher : Hasher
    {

        public override Int16 HashMethod(string str) {
            var hash = base.HashMethod(str);
            hash = ClearAskFlag(hash);
            return hash;
        }

        short ClearAskFlag(short methodHash) {
            methodHash = (short)(methodHash << 1);
            return SetAskBit(methodHash, false);
        }

        public static short SetAskBit(short methodHash, bool isAsk) {

            int methodHashInt = methodHash;
            var setAskBit = methodHashInt | (isAsk? 1 : 0);

            return (short)setAskBit;

        }
    }
}
