using System;
using System.Text;
using System.Threading.Tasks;
using TachyonCommon;

namespace Interop
{
    [Interop]
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