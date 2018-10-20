using TachyonClientRPC;
using UnityEngine.UI;
using TachyonCommon;
using UnityEngine;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Client))]
public class TachyonClient : MonoBehaviour {

    [SerializeField]
    Text _text = null;

    RPCClient _client;

    static System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();

    public void Start() {

        var clientCore = GetComponent<Client>();
        _client = new RPCClient(clientCore, new JsonSerializer());

        _client.OnConnected += () => Debug.Log("Connected to server.");
        _client.OnFailedToConnect += () => Debug.Log("FailedToConnect");
        _client.OnDisconnected += () => Debug.Log("Disconnected");
        _client.Connect("127.0.0.1", 13);

        _client.OnRecieved += OnRecieved;
        _client.On<LogDTO>(Log);
        _client.On<LogDTO>(LogWarning);
    }

    private void LogWarning(LogDTO message) {
        _text.text += "Warning: " + message.Message + "\n";
    }

    public void Log(LogDTO message) {
        _text.text += message.Message + "\n";
    }

    public void OnRecieved(byte[] bytes) {
        var str = Encoding.ASCII.GetString(bytes);
        _text.text += str + "\n";
    }

    public void Send() {
        var logDTO = new LogDTO() { Message = "Unity speaks too!" };
        _client.Send("Log", logDTO);
    }

    public void Ping() {
        _sw.Restart();
        _client.Ask<long>("Ping", Pong, DateTime.Now.Ticks);
    }

    private static void Pong(long ticks) {
        Debug.Log("Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms");
        Debug.Log("Round Trip: " + _sw.ElapsedMilliseconds + "ms");
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