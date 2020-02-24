using Interop;
using Interop;
using TachyonServerCore;
using TachyonServerRPC;
using System;
using System.Threading.Tasks;
using TachyonCommon;
namespace GeneratedBindings
{
    [HostBindingAttribute(typeof(IExampleService))]
    public class IExampleServiceHostBinding
    {
        IExampleService _service;
        HostCore _connection;
        public void Bind(HostCore connection, IExampleService service, EndPointMap endPoints )
        {
            _connection = connection;
            _service = service;
            BindClientCallbacks();
            BindHostMethodEndPoints(endPoints);
        }

        void BindClientCallbacks()
        {
            _service.OnLogWarning += (p) => _connection.Broadcast("HandleOnLogWarning", p);
            _service.OnLog += (p) => _connection.Broadcast("HandleOnLog", p);
        }

        void BindHostMethodEndPoints(EndPointMap map)
        {
            map.AddAskEndpoint<long,long>((c,m) => Ping(m).Result, "Ping");
            map.AddSendEndpoint<LogDto>((c,m) => Log(m), "Log");
        }

        Task<long> Ping(Int64 clientTime)
        {
            return _service.Ping(clientTime);
        }

        void Log(LogDto log)
        {
            _service.Log(log);
        }

    }

}

