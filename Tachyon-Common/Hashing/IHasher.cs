using System;
using System.Collections.Generic;
using System.Text;

namespace TachyonCommon.Hash
{
    public interface IHasher
    {
        short HashMethod(string str);
    }
}