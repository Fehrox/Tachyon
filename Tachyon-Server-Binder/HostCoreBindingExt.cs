using System;
using System.Linq;
using System.Reflection;
using TachyonServerCore;

namespace TachyonServerBinder
{
    public static class HostCoreBindingExt
    {
        public static void Bind<TService>(
            this HostCore host,
            TService service
        ) where TService : class
        {
            if (!service.GetType().IsInterface)
            {
                var interfaceType = service.GetType().GetInterfaces()
                    .First(i => i.IsAssignableFrom(typeof(TService)));
                HostBinder<TService>.Bind(host, interfaceType, service, host._endPoints);
            }
            else
            {
                HostBinder<TService>.Bind(host, service, host._endPoints);
            }
        }
    }
}