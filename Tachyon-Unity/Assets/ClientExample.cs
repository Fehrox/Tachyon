using TachyonClientRPC;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System;
using Interop;

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

        
//        var exampleController = new ExampleController();

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

}
