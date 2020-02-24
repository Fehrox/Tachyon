using System;
using System.Linq;
using TachyonClientRPC;

namespace TachyonClientBinder {
    public class ClientRuntimeBinder<TService> where TService : class {
        public static TService Bind(ClientRpc clientRpc) {
            var asm = ClientBindingWriter<TService>.GenerateAssembly();

            var binderType = asm.GetExportedTypes().Single();
            var serviceInstance = Activator.CreateInstance(binderType);

            var bindMethod = serviceInstance.GetType().GetMethod("Bind");
            bindMethod?.Invoke(serviceInstance, new[] {clientRpc});

            return (TService) serviceInstance;
        }
    }
}