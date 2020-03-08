using System.Collections;
using TachyonClientRPC;
using UnityEngine;
using System.Text;
using Interop;
using TachyonIO;
using TachyonServerCore;

[RequireComponent(typeof(TachyonUnityClientConnection))]
public class ClientExample : MonoBehaviour {

    ClientRpc _client;

    public IEnumerator Start() {

        var host = FindObjectOfType<TachyonUnityHost>();
        while (!host.Started)
            yield return null;
        
        
        var clientCore = GetComponent<TachyonUnityClientConnection>();
        _client = new ClientRpc(clientCore, new Interop.ManualSerializer());

        _client.OnConnected += () => Debug.Log("Connected to server.");
        _client.OnFailedToConnect += (ex) => Debug.Log("FailedToConnect " + ex.Message);
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
