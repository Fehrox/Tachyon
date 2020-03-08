using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TachyonClientIO
{
    public delegate void RecievedEvent(byte[] data);

    public delegate void ConnectionEvent();

    public delegate void FailedConnectionEvent(ConnectionFailedException ex);

    public class ConnectionFailedException : Exception {
        
        private readonly Exception _ex;
        public override string Message => _ex.Message;
        public override string StackTrace => _ex.StackTrace;
        public override IDictionary Data => _ex.Data;
        public override string Source => _ex.Source;
        public override string HelpLink => _ex.HelpLink;

        public ConnectionFailedException(SocketException socketException) {
             _ex = socketException;
         }
         
         public ConnectionFailedException(string message) {
             _ex = new Exception(message);
        }
    }

    public interface IClient
    {
        void Connect(string host, int port);
        void Send(byte[] data);

        event RecievedEvent OnRecieved;
        event ConnectionEvent OnDisconnected;
        event ConnectionEvent OnConnected;
        event FailedConnectionEvent OnFailedToConnect;
    }
}