using System;
using System.Collections.Generic;
using System.Text;

namespace TachyonClientIO
{
    public delegate void RecievedEvent(byte[] data);

    public delegate void ConnectionEvent();

    public interface IClient
    {
        void Connect(string host, int port);
        void Send(byte[] data);

        RecievedEvent OnRecieved { get; set; }
        ConnectionEvent OnDisconnected { get; set; }
        ConnectionEvent OnConnected { get; set; }
        ConnectionEvent OnFailedToConnect { get; set; }
    }
}