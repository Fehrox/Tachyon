using TachyonClientIO;
using TachyonClientRPC;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Interop;
using TachyonClientBinder;

namespace ClientProgram
{
    internal class Program
    {
        private static Stopwatch _sw = new Stopwatch();

        private static void Main(string[] args)
        {
            var client = new RPCClient(new Client(), new JsonSerializer());
            client.OnConnected += () => Console.WriteLine("Connected to server.");
            client.OnFailedToConnect += () => Console.WriteLine("FailedToConnect");
            client.OnDisconnected += () => Console.WriteLine("Disconnected");
            client.Connect("localhost", 13);

            var service = client.Bind<IExampleService>();
            
            service.OnLog += HandleOnLog;
            service.OnLogWarning += HandleOnLogWarning;
            
            Console.WriteLine("Ready to send.");
            var i = 0;
            while (true)
            {
                Console.ReadKey();

                Console.WriteLine("Sending!");
                _sw.Restart();
                var ping = service.Ping(DateTime.Now.Ticks);
                Task.Run(() => Pong(ping.Result));
                service.Log(new LogDTO {Message = "Console speaks! " + i++});
            }
        }

        private static void Pong(long ticks)
        {
            Console.WriteLine("Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms");
            Console.WriteLine("Round Trip: " + _sw.ElapsedMilliseconds + "ms");
            _sw.Stop();
        }

        public static void HandleOnLog(LogDTO message)
        {
            Console.WriteLine(message.Message);
        }

        public static void HandleOnLogWarning(LogDTO message)
        {
            Console.WriteLine("Warning: " + message.Message);
        }
    }
}