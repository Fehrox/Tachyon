using ClientProgram;
using TachyonClientIO;
using TachyonClientRPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TachyonCommon;

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

            client.On<LogDTO>(Log);
            client.On<LogDTO>(LogWarning);

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

        public static void Log(LogDTO message) {
            Console.WriteLine(message.Message);
        }

        public static void LogWarning(LogDTO message) {
            Console.WriteLine("Warning: " + message.Message);
        }
    }

    internal class JsonSerializer : ISerializer {

        public object[] DeserializeObject(byte[] obj, Type[] t) {
            var argJsonStr = Encoding.ASCII.GetString(obj);
            var jObj = JsonConvert.DeserializeObject(argJsonStr);
            if (jObj is JArray) {

                var jObjArr = jObj as JArray;
                List<object> objs = new List<object>();
                for (int i = 0; i < jObjArr.Count; i++)
                    objs.Add(jObjArr[i].ToObject(t[i]));

                return objs.ToArray();
            } else {
                return new[] { jObj };
            }
        }

        public byte[] SerializeObject<T>(T obj) {
            var replyJson = JsonConvert.SerializeObject(obj);
            var replyArgData = Encoding.ASCII.GetBytes(replyJson);
            return replyArgData;
        }
        
    }

    public class LogDTO {
        //[JsonProperty("msg")]
        public string Message;
    }
}
