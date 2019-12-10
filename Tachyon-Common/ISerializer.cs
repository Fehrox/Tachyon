using System;
using System.Collections.Generic;

namespace TachyonCommon
{
    public interface ISerializer
    {
        byte[] SerializeObject<T>(T obj);
        object[] DeserializeObject(byte[] obj, Type[] t);
    }
}