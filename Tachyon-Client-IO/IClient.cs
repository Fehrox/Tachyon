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

        event RecievedEvent OnRecieved;
        event ConnectionEvent OnDisconnected;
        event ConnectionEvent OnConnected;
        event ConnectionEvent OnFailedToConnect;
    }
}