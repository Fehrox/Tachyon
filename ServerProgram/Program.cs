using TachyonServerCore;
using TachyonServerRPC;
using System;
using Interop;
using TachyonServerBinder;

namespace ServerProgram
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var endPointMap = new EndPointMap(new JsonSerializer());
            var host = new HostCore(endPointMap);
            var service = new ExampleService();
            host.Bind(service);
            host.Start();
            Console.WriteLine("Server Started.");

            while (true)
            {
                service.Update();
                Console.ReadKey();
            }
        }
    }
}