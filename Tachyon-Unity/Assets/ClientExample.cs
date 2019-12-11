using TachyonClientRPC;
using UnityEngine.UI;
using TachyonCommon;
using UnityEngine;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(TachyonUnityConnection))]
public class ClientExample : MonoBehaviour {

    [SerializeField]
    Text _text = null;

    ClientRpc _client;

    static System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();

    public void Start() {

        var clientCore = GetComponent<TachyonUnityConnection>();
        _client = new ClientRpc(clientCore, new JsonSerializer());

        _client.OnConnected += () => Debug.Log("Connected to server.");
        _client.OnFailedToConnect += () => Debug.Log("FailedToConnect");
        _client.OnDisconnected += () => Debug.Log("Disconnected");
        _client.Connect("127.0.0.1", 13);

        _client.OnRecieved += OnRecieved;
        _client.On<LogDTO>(HandleOnLog);
        _client.On<LogDTO>(HandleOnLogWarning);
    }

    private void HandleOnLogWarning(LogDTO message) {
        _text.text += "Warning: " + message.Message + "\n";
    }

    public void HandleOnLog(LogDTO message) {
        _text.text += message.Message + "\n";
    }

    public void OnRecieved(byte[] bytes) {
        var str = Encoding.ASCII.GetString(bytes);
        Console.WriteLine("Received: " + str);
//        _text.text += str + "\n";
    }

    public void Send() {
        var logDTO = new LogDTO() { Message = "Unity speaks too!" };
        _client.Send("Log", logDTO);
    }

    public void Ping() {
        _sw.Restart();
        _client.Ask<long>("Ping", Pong, DateTime.Now.Ticks);
    }

    private void Pong(long ticks) {
        _text.text += "Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms" + "\n";
        _text.text += "Round Trip: " + _sw.ElapsedMilliseconds + "ms" + "\n";
        _sw.Stop();
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


}

public class LogDTO {
    //[JsonProperty("msg")]
    public string Message;
}