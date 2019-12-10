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
            var server = new HostCore(endPointMap);
            var service = new ExampleService();
            server.Bind(service);
            server.Start();
            Console.WriteLine("Server Started.");

            while (true)
            {
                service.Update();
                Console.ReadKey();
            }
        }
    }
}