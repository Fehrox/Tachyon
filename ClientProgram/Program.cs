using TachyonClientIO;
using TachyonClientRPC;
using System;
using Interop;
using TachyonClientBinder;

namespace ClientProgram
{
    internal static class Program
    {

        private static void Main(string[] args)
        {
            var client = new RPCClient(new Client(), new JsonSerializer());
            client.OnConnected += () => Console.WriteLine("Connected to server.");
            client.OnFailedToConnect += () => Console.WriteLine("FailedToConnect");
            client.OnDisconnected += () => Console.WriteLine("Disconnected");
            client.Connect("localhost", 13);

            var service = client.Bind<IExampleService>();
            var controller = new ExampleController(service);
            Console.WriteLine("Ready to send.");
            
            while (true)
            {            
                Console.ReadKey();
                controller.ExampleClientBehaviour();
            }
        }


    }
}