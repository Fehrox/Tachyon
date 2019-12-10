using System;
using System.Threading.Tasks;

namespace Interop
{
    namespace Interop
    {
        public interface IExampleService
        {
            event Action<LogDTO> OnLogWarning;

            event Action<LogDTO> OnLog;

            Task<long> Ping(long ticks);

            void Log(LogDTO log);
        }
    }
}