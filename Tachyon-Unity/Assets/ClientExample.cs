using TachyonClientRPC;
using UnityEngine;
using System.Text;
using Interop;

[RequireComponent(typeof(TachyonUnityConnection))]
public class ClientExample : MonoBehaviour {

    ClientRpc _client;

    public void Start() {

        var clientCore = GetComponent<TachyonUnityConnection>();
        _client = new ClientRpc(clientCore, new Interop.ManualSerializer());

        _client.OnConnected += () => Debug.Log("Connected to server.");
        _client.OnFailedToConnect += () => Debug.Log("FailedToConnect");
        _client.OnDisconnected += () => Debug.Log("Disconnected");
        _client.Connect("127.0.0.1", 13);
        
        var service = _client.Bind<IExampleService>();
        FindObjectOfType<ExampleController>().Inject(service);

        _client.OnRecieved += OnRecieved;
    }

    void OnRecieved(byte[] bytes) {
        var str = Encoding.ASCII.GetString(bytes);
        Debug.Log("Received: " + str);
    }

}
