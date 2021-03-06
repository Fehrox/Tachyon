using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;
using TachyonCommon;
using TachyonServerCore;
using TachyonServerRPC;

namespace TachyonServerBinder {
    public class HostBindingWriter<TService> where TService : class {
        
        public static string GenerateCode() {
            SourceWriter x = new SourceWriter();
            WriteCode(x);
            return x.Code();
        }

        public static Assembly GenerateAssembly() {
            var serviceInterfaceType = typeof(TService);
            
            if (!serviceInterfaceType.IsInterface)
                throw new ArgumentException(
                    $"{serviceInterfaceType.Name} is not an interface. " +
                            $"Use interfaces to define the communication contracts, " +
                            $"rather than binding a class. ");

            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(typeof(TService).Assembly);
            generator.ReferenceAssembly(serviceInterfaceType.Assembly);
            generator.ReferenceAssembly(typeof(HostCore).Assembly);
            generator.ReferenceAssembly(typeof(EndPointMap).Assembly);
            generator.ReferenceAssembly(typeof(Console).Assembly);
            generator.ReferenceAssembly(typeof(Task<>).Assembly);
            generator.ReferenceAssembly(typeof(HostBindingAttribute).Assembly);

            var assembly = generator.Generate(WriteCode);
            
            return assembly;
        }

        public static void WriteCode(ISourceWriter x) {
            var serviceInterfaceType = typeof(TService);

            x.UsingNamespace(typeof(TService).Namespace);
            x.UsingNamespace(serviceInterfaceType.Namespace);
            x.UsingNamespace(typeof(HostCore).Namespace);
            x.UsingNamespace(typeof(EndPointMap).Namespace);
            x.UsingNamespace(typeof(Console).Namespace);
            x.UsingNamespace(typeof(Task<>).Namespace);
            x.UsingNamespace(typeof(HostBindingAttribute).Namespace);

            // Start class/namespace
            x.Namespace("GeneratedBindings");
            x.WriteLine($"[{typeof(HostBindingAttribute).Name}(typeof({typeof(TService).Name}))]");
            x.StartClass( typeof(TService).Name + "HostBinding" );

            // Fields
            x.Write($"{typeof(TService).Name} _service;");
            x.Write($"{typeof(HostCore).Name} _connection;");

            // Methods
            // Bind()
            x.Write($"BLOCK:public void Bind(" +
                    $"{typeof(HostCore).NameInCode()} connection, " +
                    $"{typeof(TService).NameInCode()} service, " +
                    $"{typeof(EndPointMap).NameInCode()} endPoints )");
            x.Write("_connection = connection;");
            x.Write("_service = service;");
            x.Write("BindClientCallbacks();");
            x.Write("BindHostMethodEndPoints(endPoints);");
            x.FinishBlock(); // Finish method Bind()

            // BindClientCallbacks()
            x.Write("BLOCK:void BindClientCallbacks()");
            GenerateBindClientCallbacks(serviceInterfaceType, x);
            x.FinishBlock(); // Finish method BindClientCallbacks()

            // BindHostMethodEndPoints()
            x.Write($"BLOCK:void BindHostMethodEndPoints({typeof(EndPointMap).Name} map)");
            GenerateBindHostMethodEndPoints(serviceInterfaceType, x);
            x.FinishBlock(); // Finish BindClientCallbacks()

            // Route Method callbacks to encapsulated service.
            GenerateBindHostMethodHandlers(serviceInterfaceType, x);

            // End class/namespace
            x.FinishBlock(); // Finish class xBinding
            x.FinishBlock(); // Finish namespace

            // var writer = (x as SourceWriter).Code();
            // Console.WriteLine(writer);
        }
        
         private static void GenerateBindHostMethodEndPoints(Type serviceType, ISourceWriter x)
        {
            var serviceMethods = GetServiceMethods(serviceType);
            foreach (var serviceMethod in serviceMethods)
            {
                var returnType = serviceMethod.ReturnType;
                var isVoidReturn = returnType == typeof(void);
                if (!isVoidReturn)
                    GenerateBindAskEndpoint(serviceMethod, x);
                else
                    GenerateBindSendEndpoint(serviceMethod, x);
            }
        }

        private static void GenerateBindSendEndpoint(MethodInfo serviceMethod, ISourceWriter x)
        {
            var requestType = serviceMethod.GetParameters().Single().ParameterType;
            var methodName = serviceMethod.Name;

            x.Write($"map.AddSendEndpoint<{requestType.NameInCode()}>(" +
                    $"(c,m) => {methodName}(m), \"{methodName}\");");
        }

        private static void GenerateBindAskEndpoint(MethodInfo serviceMethod, ISourceWriter x)
        {
            var returnType = serviceMethod.ReturnType.GetGenericTypeDefinition();
            var returnIsTask = returnType == typeof(Task<>);
            if (!returnIsTask) return;

            var taskResponseType = serviceMethod.ReturnType.GenericTypeArguments[0];
            var requestType = serviceMethod.GetParameters().Single().ParameterType;
            var methodName = serviceMethod.Name;

            x.Write($"map.AddAskEndpoint<{requestType.NameInCode()},{taskResponseType.NameInCode()}>(" +
                $"(c,m) => {methodName}(m).Result, \"{methodName}\");");
        }

        private static void GenerateBindClientCallbacks(Type serviceType, ISourceWriter x)
        {
            foreach (var eventInfo in serviceType.GetEvents())
            {
                var eventName = eventInfo.Name;
                x.Write($"_service.{eventName} += " +
                    $"(p) => _connection.Broadcast(\"Handle{eventName}\", p);");
            }
        }

        private static void GenerateBindHostMethodHandlers(Type serviceType, ISourceWriter x)
        {
            var serviceMethods = GetServiceMethods(serviceType);
            foreach (var methodInfo in serviceMethods)
            {
                var parameters = methodInfo.GetParameters();
                var paramStr = string.Join(
                    ",",
                    parameters.Select((p) => p.ParameterType.Name + " " + p.Name));
                var argStr = string.Join(
                    ",",
                    parameters.Select((p) => p.Name));
                var returnType = methodInfo.ReturnType;

                // Method name
                x.Write($"BLOCK:{returnType.NameInCode()} {methodInfo.Name}({paramStr})");

                var isVoidReturn = returnType == typeof(void);
                var returnStr = isVoidReturn ? "" : "return ";

                x.Write(returnStr + $"_service.{methodInfo.Name}({argStr});");
                x.FinishBlock(); // Finish method
            }
        }

        private static IEnumerable<MethodInfo> GetServiceMethods(Type serviceType)
        {
            var serviceMethods = serviceType
//                .GetInterface(typeof(TService).Name)
                .GetMethods()
                .Where(m => !m.IsSpecialName);
            return serviceMethods;
        }
        
    }
}