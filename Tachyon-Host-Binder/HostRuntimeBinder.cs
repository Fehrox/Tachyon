using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;
using TachyonServerCore;
using TachyonServerRPC;

namespace TachyonServerBinder
{
    public static class HostRuntimeBinder<TService> where TService : class
    {

        public static void Bind(HostCore connection, TService service, EndPointMap map) {

            var assembly = HostBindingWriter<TService>.GenerateAssembly();

            var binder = assembly.GetExportedTypes().Single();
            var bindable = Activator.CreateInstance(binder);
            
            var bindMethod = bindable.GetType().GetMethod("Bind");
            bindMethod?.Invoke(bindable, new object[] {connection, service, map});
        }
        
    }
}