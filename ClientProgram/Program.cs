using TachyonClientIO;
using TachyonClientRPC;
using System;
using System.Diagnostics;
using Interop;

namespace ClientProgram {
    class Program {

        static Stopwatch _sw = new Stopwatch();

        static void Main(string[] args) {

            var client = new RPCClient(new Client(), new JsonSerializer());
            client.OnConnected += () => Console.WriteLine("Connected to server.");
            client.OnFailedToConnect += () => Console.WriteLine("FailedToConnect");
            client.OnDisconnected += () => Console.WriteLine("Disconnected");
            client.Connect("localhost", 13);

            //client.OnRecieved += (bytes) => {
            //    var str = Encoding.ASCII.GetString(bytes);
            //    Console.WriteLine(str);
            //};

            client.On<LogDTO>(HandleOnLog);
            client.On<LogDTO>(HandleOnLogWarning);

            int i = 0;
            while (true) {
                Console.ReadKey();

                _sw.Restart();
                client.Ask<long>("Ping", Pong, DateTime.Now.Ticks);
                client.Send("Log", new LogDTO { Message = "Console speaks! " + i++ });

            }

        }

        private static void Pong(long ticks) {
            Console.WriteLine("Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms" );
            Console.WriteLine("Round Trip: " + _sw.ElapsedMilliseconds + "ms");
            _sw.Stop();
        }

        public static void HandleOnLog(LogDTO message) {
            Console.WriteLine(message.Message);
        }

        public static void HandleOnLogWarning(LogDTO message) {
            Console.WriteLine("Warning: " + message.Message);
        }
    }




}
