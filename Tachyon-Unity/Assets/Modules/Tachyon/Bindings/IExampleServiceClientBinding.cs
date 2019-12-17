using Interop;
using TachyonClientRPC;
using System.Threading.Tasks;
using System;
namespace GeneratedBindings
{
    public class IExampleServiceClientBinding : Interop.IExampleService
    {
        public event Action<Interop.LogDto> OnLogWarning;
        public event Action<Interop.LogDto> OnLog;
        ClientRpc _client;
        public void Bind(ClientRpc client)
        {
            client.On<LogDto>(HandleOnLogWarning);
            client.On<LogDto>(HandleOnLog);
            _client = client;
        }

        public void HandleOnLogWarning(LogDto logdtoArg)
        {
            OnLogWarning?.Invoke(logdtoArg);
        }

        public void HandleOnLog(LogDto logdtoArg)
        {
            OnLog?.Invoke(logdtoArg);
        }

        public Task<long> Ping(Int64 clientTimeArg)
        {
            return _client.AskTask<long>("Ping",clientTimeArg);
        }

        public void Log(LogDto logArg)
        {
            _client.Send("Log", logArg);
        }

    }

}

