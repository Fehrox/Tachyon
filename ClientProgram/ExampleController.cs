using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Interop;

namespace ClientProgram
{
    public class ExampleController
    {
        private IExampleService _service;
        
        int _i = 0;
        private static Stopwatch _sw = new Stopwatch();

        public ExampleController(IExampleService service)
        {
            _service = service;
            service.OnLog += HandleOnLog;
            service.OnLogWarning += HandleOnLogWarning;
        }

        public void ExampleClientBehaviour()
        {
            Console.WriteLine("Sending!");
            _sw.Restart();
           
            var ping = _service.Ping(DateTime.Now.Ticks);
            Pong(ping);
            
            _service.Log(new LogDto {Message = "Console speaks! " + _i++});
        }

        private static async void Pong(Task<long> ping)
        {
            var ticks = await ping;
            Console.WriteLine("Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms");
            Console.WriteLine("Round Trip: " + _sw.ElapsedMilliseconds + "ms");
            _sw.Stop();
        }

        public static void HandleOnLog(LogDto message)
        {
            Console.WriteLine(message.Message);
        }

        public static void HandleOnLogWarning(LogDto message)
        {
            Console.WriteLine("Warning: " + message.Message);
        }
    }
}