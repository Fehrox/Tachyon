using System.Collections.Generic;
using TachyonClientCommon;
using TachyonCommon.Hash;
using System;
using TachyonCommon;

namespace TachyonServerRPC {

    public class EndpointMap {
        
        public ServerStub Stub;
        public ISerializer Serializer;
        public AskHasher Hasher = new AskHasher();

        List<Action<ConnectedClient>> _sendEndPoints = new List<Action<ConnectedClient>>();
        List<Action<ConnectedClient>> _askEndPoints = new List<Action<ConnectedClient>>();

        public EndpointMap(ISerializer serializer) {
            Serializer = serializer;
            Stub = new ServerStub(serializer);
        }

        /// <summary>
        /// Register a function which may be called by the client.
        /// </summary>
        /// <typeparam name="T">Type of the method's argument.</typeparam>
        /// <param name="endPoint">The method called by the client.</param>
        public void AddSendEndpoint<T>(Action<ConnectedClient, T> endPoint) {

            _sendEndPoints.Add((client) => {
                Action<T> method = new Action<T>((parameters) => endPoint.Invoke(client, parameters));
                var methodHash = Hasher.HashMethod(endPoint.Method.Name);
                Stub.Register<T>(methodHash, method, client.Guid);
            });

        }

        /// <summary>
        /// Register a function which may be called by the client, to which the server will respond with data.
        /// </summary>
        /// <typeparam name="I">Request parameter type</typeparam>
        /// <typeparam name="O">Response data type</typeparam>
        /// <param name="clientMethod">Method which will handle the request.</param>
        public void AddAskEndpoint<I, O>(Func<ConnectedClient, I, O> clientMethod) {

            _askEndPoints.Add((client) => {
                Func<I, O> func = new Func<I, O>((parameters) => clientMethod.Invoke(client, parameters));
                var methodHash = Hasher.HashMethod(clientMethod.Method.Name);
                methodHash = AskHasher.SetAskBit(methodHash, true);
                Stub.Register<I, O>(methodHash, func, client.Guid);
            });

        }

        /// <summary>
        /// Register this connection to participate in endpoint interractions.
        /// </summary>
        /// <param name="connection">Connected to connect to endpoints.</param>
        public void RegisterConnection(ConnectedClient connection) {

            foreach (var sendEndPoint in _sendEndPoints)
                sendEndPoint.Invoke(connection);

            foreach (var askEndpoint in _askEndPoints)
                askEndpoint.Invoke(connection);

        }
    }
}
