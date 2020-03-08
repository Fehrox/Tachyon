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
            HostRuntimeBinder<TService>.Bind(host, service, host.EndPoints);
        }
    }
}