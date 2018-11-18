using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TachyonServerCore;
using TachyonServerRPC;
using Newtonsoft.Json;
using TachyonCommon;
using System.Text;
using System;

namespace ServerProgram
{
    class Program
    {
        static void Main(string[] args) {

            var endPointMap = new EndpointMap(new JsonSerializer());
            endPointMap.AddSendEndpoint<LogDTO>(Log);
            endPointMap.AddAskEndpoint<long, long>(Ping);

            var server = new HostCore(endPointMap);
            server.Start();

            Console.WriteLine("Server Started.");
            TestBroadcast(server);
        }

        private static void TestBroadcast(HostCore server) {
            int i = 0;
            while (true) {

                var log = new LogDTO {
                    //Message = "Short." };
                    Message = "This is some message " + (i++) + "! " +
                    "But it's really really long, because we need to " +
                    "test if the message can wrap it's buffer size " +
                    "and still be correctly reconstructed at the other " +
                    "end of the network. Because althought this protocol " +
                    "isn't optimised for large packets, we still want to " +
                    "be able to fully support them. Doing so makes this " +
                    "system much more flexible, and allows the sending of " +
                    "files, like images, video, binary updates, whatever " +
                    "you can imagine, it can send. I know that's a pretty " +
                    "standard feature, but the way other networking systems " +
                    "are built, they support this kind of thing at the expense " +
                    "of performance with smaller data packets. Not Tachyon, " +
                    "tachyon can send and recieve those tiny packets faster " +
                    "than the speed of light, and still handle the big ones."};
            if (i % 2 == 0)
                    server.Broadcast("Log", log);
                else
                    server.Broadcast("LogWarning", new LogDTO { Message = "." });

                Console.ReadKey();
            }
        }

        private static long Ping(ConnectedClient client, long clientTime) {
            return DateTime.Now.Ticks - clientTime;
        }

        private static void Log(ConnectedClient client, LogDTO message) {
            Console.WriteLine(client.GetHashCode() + ": " + message.Message);
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

                if (jObj is JObject && jObj.Equals(default(JObject)))
                    return new object[0];

                return new[] { (jObj as JObject).ToObject(t[0]) };
            }
        }

        public byte[] SerializeObject<T>(T obj) {
            var replyJson = JsonConvert.SerializeObject(obj);
            var replyArgData = Encoding.ASCII.GetBytes(replyJson);
            return replyArgData;
        }

    }

    public class LogDTO {
        public string Message;
    }

}
