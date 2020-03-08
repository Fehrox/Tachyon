using System;
using System.Diagnostics;
using ManualSerializer;
using Interop;
using UnityEngine;
using UnityEngine.UI;

public class ExampleController : MonoBehaviour            
{
    private IExampleService _service;
    
    [SerializeField]
    Text _text = null;
    
    private static Stopwatch _sw = new Stopwatch();

    public void Inject(IExampleService service) {  
        _service = service;
        service.OnLog += HandleOnLog;
        service.OnLogWarning += HandleOnLogWarning;
    }
    
    public void HandleOnLog(LogDto message) {
        _text.text += message.Message + "\n";
    }
 
    private void HandleOnLogWarning(LogDto message) {
        _text.text += "Warning: " + message.Message + "\n";
    }
    
    public void Send() {
        var logDTO = new LogDto() { Message = "Unity speaks too!" };
        _service.Log(logDTO);
    }
    
    public void Ping() {
        _service
            .Ping(DateTime.Now.Ticks)
            .Then(Pong);
    }

    private void Pong(long ticks)
    {
        _sw.Restart();
        _text.text += "Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms" + "\n";
        _text.text += "Round Trip: " + _sw.ElapsedMilliseconds + "ms" + "\n";
        _sw.Stop();
    }
    
}