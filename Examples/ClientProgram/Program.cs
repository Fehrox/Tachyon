using TachyonClientIO;
using TachyonClientRPC;
using System;
using Interop;
using TachyonClientBinder;

namespace ClientProgram
{
    internal static class Program
    {

        private static void Main()
        {
            var client = new ClientRpc(new Client(), new ManualSerializer());
            client.OnConnected += () => Console.WriteLine("Connected to server.");
            client.OnFailedToConnect += (ex) => Console.WriteLine("FailedToConnect " + ex.Message);
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