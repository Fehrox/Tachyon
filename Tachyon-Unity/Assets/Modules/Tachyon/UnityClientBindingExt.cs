using System;
using System.Linq;
using System.Reflection;
using TachyonClientRPC;

public static class UnityClientBindingExt 
{
    public static TService Bind<TService>(this ClientRpc client) where TService : class
    {
        var serviceType = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Single(t => 
                typeof(TService).IsAssignableFrom(t) &&
                !t.IsInterface );
        
        var bindMethod = serviceType.GetMethod("Bind");
        
        var service = Activator.CreateInstance(serviceType);
        bindMethod?.Invoke(service, new[] {client});

        return service as TService;
    }
}
