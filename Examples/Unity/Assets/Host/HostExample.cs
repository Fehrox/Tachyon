using Host;
using Interop;
using TachyonServerCore;
using UnityEngine;

[RequireComponent(
    typeof(ExampleService),
    typeof(TachyonUnityHost))]
public class HostExample : MonoBehaviour
{
    
    void Start() {
        
        var host = GetComponent<TachyonUnityHost>();
        var service = GetComponent<IExampleService>();
        host.Initialize( new Interop.ManualSerializer(), service );
        
        host.OnClientConnected += connection => Debug.Log(connection + " connected.");
        host.OnClientDisconnected += connection => Debug.Log(connection + " disconnected.");
        
    }

}


