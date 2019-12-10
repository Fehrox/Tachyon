using System;
using System.Threading.Tasks;

namespace Interop
{
    public interface IExampleService
    {
        event Action<LogDTO> OnLogWarning;

        event Action<LogDTO> OnLog;

        Task<long> Ping(long clientTime);

        void Log(LogDTO log);
    }
    
    public class LogDTO {
        public string Message;
    }
    
}