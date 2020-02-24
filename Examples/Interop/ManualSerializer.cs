using TachyonCommon;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Interop
{
    public class ManualSerializer : ISerializer
    {
        
        public byte[] SerializeObject<T>(T obj)
        {
            if(obj is IManualSerializer packObj)
                return packObj.Pack();

            if (obj is Int64 packInt64) {
                var sendBytes = BitConverter.GetBytes(packInt64);
                return sendBytes;
            }
            
            throw new ArgumentException(
                $"Provided object of type " +
                $"{obj.GetType()}, does not inherit " +
                $"{typeof(IManualSerializer).Name}"
            );
        }

        public object DeserializeObject(byte[] objBytes, Type type) {
            
            if (type.GetInterfaces().Contains(typeof(IManualSerializer))) {
                var deserialized = Activator.CreateInstance(type) as IManualSerializer;
                deserialized?.Unpack(objBytes);
                return deserialized;
            }

            if (type == typeof(Int64)) 
                return BitConverter.ToInt64(objBytes, 0);
            
            throw new ArgumentException(
                    $"Object of type {type} did not " +
                    $"inherit {typeof(IManualSerializer).Name}.");
            
        }
    }

    public interface IManualSerializer
    {
        byte[] Pack();
        void Unpack(byte[] packedObj);
    }
}