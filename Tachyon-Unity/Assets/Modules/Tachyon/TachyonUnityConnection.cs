using TachyonClientIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TachyonUnityConnection : MonoBehaviour, IClient {

    public RecievedEvent OnRecieved { get; set; }

    public ConnectionEvent OnDisconnected { get; set; }
    public ConnectionEvent OnConnected { get; set; }
    public ConnectionEvent OnFailedToConnect { get; set; }

    bool _disconnectTriggered;
    bool _connectedTriggered;
    bool _failedToConnectTriggered;

    TachyonClientIO.Client _client;

    Queue<byte[]> _recievedQueue = new Queue<byte[]>(); 

    IEnumerator Start() {
        while (true) {

            if (_disconnectTriggered) {
                OnDisconnected?.Invoke();
                _disconnectTriggered = false;
            }

            if (_connectedTriggered) {
                OnConnected?.Invoke();
                _connectedTriggered = false;
            }

            if (_failedToConnectTriggered) {
                OnFailedToConnect?.Invoke();
                _failedToConnectTriggered = false;
            }
            
            while (_recievedQueue.Count > 0) {
                var nextRecieved = _recievedQueue.Dequeue();
                OnRecieved?.Invoke(nextRecieved);
            }

            yield return null;
        }


    }

    public void Connect(string host, int port) {

        _client = new TachyonClientIO.Client();

        _client.OnConnected += () => _connectedTriggered = true;
        _client.OnDisconnected += () => _disconnectTriggered = true;
        _client.OnFailedToConnect += () => _failedToConnectTriggered = true;
        _client.OnRecieved += (message) => {
            _recievedQueue.Enqueue(message);
        };

        _client.Connect(host, port);

    }

    public void Send(byte[] data) {
        _client.Send(data);
    }
    
}
