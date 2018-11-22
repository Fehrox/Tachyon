using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TachyonCommon;

internal class JsonSerializer : ISerializer {

	public object[] DeserializeObject(byte[] obj, Type[] t) {
		var argJsonStr = Encoding.ASCII.GetString(obj);
        //var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        var jObj = JsonConvert.DeserializeObject(argJsonStr);
		if (jObj is JArray) {

			var jObjArr = jObj as JArray;
			List<object> objs = new List<object>();
			for (int i = 0; i < jObjArr.Count; i++)
				objs.Add(jObjArr[i].ToObject(t[i]));

			return objs.ToArray();
		} else {
            var typedObj = (jObj as JObject).ToObject(t[0]);
			return new object [] { typedObj };
		}
	}

	public byte[] SerializeObject<T>(T obj) {
		var replyJson = JsonConvert.SerializeObject(obj);
		var replyArgData = Encoding.ASCII.GetBytes(replyJson);
		return replyArgData;
	}

}