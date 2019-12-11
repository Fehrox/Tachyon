using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;
using TachyonClientRPC;

namespace TachyonClientBinder
{
    public static class ClientBindingWriter<TService> where TService : class
    {
        
        public static string GenerateCode()
        {
            SourceWriter x = new SourceWriter();
            WriteCode(x);
            return x.Code();
        }

        public static Assembly GenerateAssembly()
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssemblyContainingType<TService>();
            generator.ReferenceAssemblyContainingType<ClientRpc>();
            generator.ReferenceAssembly(typeof(Task<>).Assembly);
            generator.ReferenceAssembly(typeof(Action<>).Assembly);

            var assembly = generator.Generate(WriteCode);
            
            return assembly;
        }

        private static void WriteCode(ISourceWriter x)
        {
            x.UsingNamespace(typeof(TService).Namespace);
            x.UsingNamespace(typeof(ClientRpc).Namespace);
            x.UsingNamespace(typeof(Task<>).Namespace);
            x.UsingNamespace(typeof(Action<>).Namespace);

            // Class/Namespace
            x.Namespace("GeneratedBindings");
            x.StartClass(typeof(TService).NameInCode() + "ClientBinding", typeof(TService));

            // Fields
            GenerateBindEvents(x);

            x.Write($"{typeof(ClientRpc).Name} _client;");

            // Methods
            // Bind()
            x.Write($"BLOCK:public void Bind({typeof(ClientRpc).NameInCode()} client)");
            GenerateBindOnCallbacks(x);
            x.Write("_client = client;");
            x.FinishBlock(); // Finish Bind();

            // HandleX(Y y)
            GenerateBindOnCallbackHandlers(x);

            // void Methods
            GenerateInterfaceMethods(x);

            x.FinishBlock(); // Finish class xBinding

            x.FinishBlock(); // Finish Class/Namespace
        }

        private static void GenerateInterfaceMethods(ISourceWriter x)
        {
            var methods = typeof(TService).GetMethods()
                .Where(m => !m.IsSpecialName);
            foreach (var methodInfo in methods)
            {
                var returnType = methodInfo.ReturnType;
                var isVoidReturn = returnType == typeof(void);
                if (!isVoidReturn)
                    GenerateBindAskDispatcher(methodInfo, x);
                else
                    GenerateBindSendDispatcher(methodInfo, x);
            }
        }
        
        private static void GenerateBindAskDispatcher(MethodInfo methodInfo, ISourceWriter x)
        {
            var parameters = methodInfo.GetParameters();
            var paramsStr = string.Join(
                ",", 
                parameters.Select(p => p.ParameterType.Name + " " + p.Name));

            var paramStr = string.Join(
                ",",
                parameters.Select(p => p.Name));
            var returnType = methodInfo.ReturnType;
            
            var paramTypeStr = string.Join(
                ",",
                parameters.Select(p => p.ParameterType.NameInCode()));
            
            x.Write($"BLOCK:public {returnType.NameInCode()} {methodInfo.Name}({paramsStr})");
            x.Write("bool finished = false;");
            x.Write( paramTypeStr + " result = default;");
            x.Write($"_client.Ask<{paramTypeStr}>(\"{methodInfo.Name}\"," +
                    $"(r) => {{result = r; finished = true;}}, {paramStr});");
            x.Write("var t = Task.Run(" +
                    "() => { " +
                        "while (!finished) Task.Yield(); " +
                        "return result; " +
                    "});");
            x.Write($"return t;");
            x.FinishBlock();
        }

        private static void GenerateBindSendDispatcher(MethodInfo methodInfo, ISourceWriter x)
        {
            var parameters = methodInfo.GetParameters();
            var paramsVarStr = string.Join(
                ",", 
                parameters.Select(p => p.ParameterType.Name + " " + p.Name));
            var paramVarStr = string.Join(
                ",",
                parameters.Select(p => p.Name));
            x.Write($"BLOCK:public void {methodInfo.Name}({paramsVarStr})");
            x.Write($"_client.Send(\"{methodInfo.Name}\", {paramVarStr});");
            x.FinishBlock();
        }

        private static void GenerateBindOnCallbackHandlers(ISourceWriter x)
        {
            foreach (var eventInfo in typeof(TService).GetEvents())
            {
                var eventParam = eventInfo.EventHandlerType.GenericTypeArguments.Single();
                var variableName = eventParam.NameInCode().ToLower();
                var className = eventParam.NameInCode();
                x.Write($"BLOCK:public void Handle{eventInfo.Name}({className} {variableName})");
                x.Write($"{eventInfo.Name}?.Invoke({variableName});");
                x.FinishBlock();
            }
        }
        
        private static void GenerateBindOnCallbacks(ISourceWriter x)
        {
            foreach (var eventInfo in typeof(TService).GetEvents())
            {
                var eventParam = eventInfo.EventHandlerType.GenericTypeArguments.Single();
                x.Write($"client.On<{eventParam.NameInCode()}>(Handle{eventInfo.Name});");
            }

        }

        private static void GenerateBindEvents(ISourceWriter x)
        {
            foreach (var eventInfo in typeof(TService).GetEvents())
            {
                var delegateType = eventInfo.EventHandlerType.NameInCode();
                x.Write("public event " + delegateType + " " + eventInfo.Name+";");
            }
        }
    }
}