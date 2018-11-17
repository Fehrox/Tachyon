using System;
using System.Collections.Generic;
using System.Text;

namespace TachyonServerIO
{

    public delegate void RecievedEvent(byte[] data);
    public delegate void ConnectionEvent();

    public interface IConnection 
    {

        void Send(byte[] data);
        void Start();

        RecievedEvent OnRecieved { get; set; }
        ConnectionEvent OnDisconnected {get;set;}

    }

}
