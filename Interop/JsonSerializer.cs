using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TachyonCommon;

namespace Interop
{
    public class JsonSerializer : ISerializer
    {
        public object[] DeserializeObject(byte[] obj, Type[] t)
        {
            var argJsonStr = Encoding.ASCII.GetString(obj);
            var jObj = JsonConvert.DeserializeObject(argJsonStr);
            if (jObj is JArray)
            {
                var jObjArr = jObj as JArray;
                var objs = new List<object>();
                for (var i = 0; i < jObjArr.Count; i++)
                    objs.Add(jObjArr[i].ToObject(t[i]));

                return objs.ToArray();
            }
            else
            {
                return new[] {jObj};
            }
        }

        public byte[] SerializeObject<T>(T obj)
        {
            var replyJson = JsonConvert.SerializeObject(obj);
            var replyArgData = Encoding.ASCII.GetBytes(replyJson);
            return replyArgData;
        }
    }
}