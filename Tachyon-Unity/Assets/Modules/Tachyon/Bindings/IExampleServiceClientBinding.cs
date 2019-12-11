using Interop;
using TachyonClientRPC;
using System.Threading.Tasks;
using System;
namespace GeneratedBindings
{
    public class IExampleServiceClientBinding : Interop.IExampleService
    {
        public event Action<Interop.LogDTO> OnLogWarning;
        public event Action<Interop.LogDTO> OnLog;
        ClientRpc _client;
        public void Bind(ClientRpc client)
        {
            client.On<LogDTO>(HandleOnLogWarning);
            client.On<LogDTO>(HandleOnLog);
            _client = client;
        }

        public void HandleOnLogWarning(LogDTO logdto)
        {
            OnLogWarning?.Invoke(logdto);
        }

        public void HandleOnLog(LogDTO logdto)
        {
            OnLog?.Invoke(logdto);
        }

        public Task<long> Ping(Int64 clientTime)
        {
            bool finished = false;
            long result = default;
            _client.Ask<long>("Ping",(r) => {result = r; finished = true;}, clientTime);
            var t = Task.Run(() => { while (!finished) Task.Yield(); return result; });
            return t;
        }

        public void Log(LogDTO log)
        {
            _client.Send("Log", log);
        }

    }

}

