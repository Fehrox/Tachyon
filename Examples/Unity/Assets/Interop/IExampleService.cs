using System.Threading.Tasks;
using TachyonCommon;
using System;

namespace Interop
{    
    [GenerateBindings]
    public interface IExampleService  
    {
        event Action<LogDto> OnLogWarning;

        event Action<LogDto> OnLog;

        Task<long> Ping(long clientTime);

        void Log(LogDto log);
    }

    public partial class LogDto 
    {
        public string Message;
    }
}