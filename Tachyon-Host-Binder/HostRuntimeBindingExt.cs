using System;
using System.Linq;
using System.Reflection;
using TachyonServerCore;

namespace TachyonServerBinder
{
    public static class RuntimeHostBindingExt
    {
        public static void Bind<TService>(
            this HostCore host,
            TService service
        ) where TService : class {
            HostRuntimeBinder<TService>.Bind(host, service, host._endPoints);
            // if (!service.GetType().IsInterface)
            // {
            //     // var interfaceType = service.GetType().GetInterfaces()
            //     //     .First(i => i.IsAssignableFrom(typeof(TService)));
            //     HostRuntimeBinder<TService>.Bind(host, service, host._endPoints);
            // }
            // else
            // {
            //     HostRuntimeBinder<TService>.Bind(host, service, host._endPoints);
            // }
        }
    }
}