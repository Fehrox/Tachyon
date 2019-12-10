using System.Collections.Generic;
using System.Reflection;
using TachyonCommon;
using System.Linq;
using System;

namespace TachyonClientCommon
{
    public class Stub
    {
        private ISerializer _serializer;
        private Dictionary<short, RemoteMethod> _methods = new Dictionary<short, RemoteMethod>();

        private const int METHOD_HEADER = 2;

        public Stub(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void Register<I, O>(short methodHash, Func<I, O> func)
        {
            var methodCallback = new RemoteMethod(func.Method, func.Target);
            Register(methodHash, methodCallback);
        }

        public void Register<T>(short methodHash, Action<T> method)
        {
            var methodCallback = new RemoteMethod(method.Method, method.Target);
            Register(methodHash, methodCallback);
        }

        private void Register(short methodHash, RemoteMethod remoteMethod)
        {
            if (!_methods.ContainsKey(methodHash))
            {
                _methods.Add(methodHash, remoteMethod);
            }
            else
            {
                var hashConflict = _methods[methodHash].Method.Name;
                var errorMsg = remoteMethod.Method.Name + " shares hash with " +
                               hashConflict + ". Rename one before registering callback.";
                throw new InvalidOperationException(errorMsg);
            }
        }

        public InvocationDescriptor GetMethod(byte[] data, short argStartIndex)
        {
            var methodHashBytes = data.Take(METHOD_HEADER).ToArray();
            var methodHash = BitConverter.ToInt16(methodHashBytes, 0);

            if (_methods.ContainsKey(methodHash))
            {
                var clientMethod = _methods[methodHash];

                var argumetData = data.Skip(argStartIndex).ToArray();
                var argTypes = clientMethod.Method.GetParameters()
                    .Select(p => p.ParameterType).ToArray();
                var args = _serializer.DeserializeObject(argumetData, argTypes);

                //var argType = args.GetType().FullName;

                return new InvocationDescriptor
                {
                    ArgumentData = args,
                    ClientMethod = clientMethod
                };
            }
            else
            {
                return null;
            }
        }

        public void Invoke(byte[] data)
        {
            var method = GetMethod(data, METHOD_HEADER);
            if (method != null) method.Invoke();
        }

        public byte[] PackSend(short methodHash, object[] arg)
        {
            var commandHeaderBytes = BitConverter.GetBytes(methodHash);
            var argData = _serializer.SerializeObject(arg);

            var sendData = new byte[commandHeaderBytes.Length + argData.Length];
            Array.Copy(commandHeaderBytes, sendData, commandHeaderBytes.Length);
            Array.Copy(argData, 0, sendData, commandHeaderBytes.Length, argData.Length);

            return sendData;
        }

        public class RemoteMethod
        {
            public object Target;
            public MethodInfo Method;

            public RemoteMethod(
                MethodInfo method,
                object target
            )
            {
                Method = method;
                Target = target;
            }
        }

        public class InvocationDescriptor
        {
            public RemoteMethod ClientMethod { get; set; }
            public object[] ArgumentData { get; set; }

            public object Invoke()
            {
                //var expectedArgs = ClientMethod.Method.GetParameters();
                //var typedArgs = new List<object>();

                //for (int i = 0; i < expectedArgs.Length; i++) {
                //    var expectedType = expectedArgs[i].ParameterType;
                //    var arg = Arguments[i];
                //    //if(arg is JObject)
                //    //var type = arg.GetType().FullName;
                //    typedArgs.Add(Convert.ChangeType(arg, expectedType));
                //}

                return ClientMethod.Method
                    .Invoke(ClientMethod.Target, ArgumentData.ToArray());
            }
        }
    }
}