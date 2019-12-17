using System;
using System.Text;
using System.Threading.Tasks;

namespace Interop
{
    public interface IExampleService
    {
        event Action<LogDto> OnLogWarning;

        event Action<LogDto> OnLog;

        Task<long> Ping(long clientTime);

        void Log(LogDto log);
    }

    public class LogDto : IManualSerializer
    {
        public string Message;
        
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