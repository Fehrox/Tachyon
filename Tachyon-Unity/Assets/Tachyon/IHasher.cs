using System;
using System.Collections.Generic;
using System.Text;

namespace TachyonCommon.Hash
{
    public interface IHasher
    {
        Int16 HashMethod(string str);
    }
}
