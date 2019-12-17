using System.Collections;
using System.Collections.Generic;
using System.Text;
using Interop;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Interop
{
    public partial class LogDto : IManualSerializer
    {
            
        public byte[] Pack()
        {
            return Encoding.ASCII.GetBytes(Message);
        }

        public void Unpack(byte[] packedObj)
        {
            Message = Encoding.ASCII.GetString(packedObj);
        }
    }
    
}
