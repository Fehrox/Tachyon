using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DefaultNamespace;
using Interop;
using UnityEngine;
using UnityEngine.UI;

public class ExampleController : MonoBehaviour
{
    private IExampleService _service;
    
    [SerializeField]
    Text _text = null;
    
    private static Stopwatch _sw = new Stopwatch();

    public void Inject(IExampleService service)
    {
        _service = service;
        service.OnLog += HandleOnLog;
        service.OnLogWarning += HandleOnLogWarning;
    }

    private static void Pong(long ticks)
    {
        UnityEngine.Debug.Log("Time to server: " + new TimeSpan(ticks).TotalMilliseconds + "ms");
        UnityEngine.Debug.Log("Round Trip: " + _sw.ElapsedMilliseconds + "ms");
        _sw.Stop();
    }

    public void HandleOnLog(LogDTO message) {
        _text.text += message.Message + "\n";
    }
 
    private void HandleOnLogWarning(LogDTO message) {
        _text.text += "Warning: " + message.Message + "\n";
    }
    
    public void Send() {
        var logDTO = new LogDTO() { Message = "Unity speaks too!" };
        _service.Log(logDTO);
    }
    
    public void Ping() {
        _service
            .Ping(DateTime.Now.Ticks)
            .Then((pong) => {
                _sw.Restart();
                _text.text += "Time to server: " + new TimeSpan(pong).TotalMilliseconds + "ms" + "\n";
                _text.text += "Round Trip: " + _sw.ElapsedMilliseconds + "ms" + "\n";
                _sw.Stop();
            });
    }
    
}