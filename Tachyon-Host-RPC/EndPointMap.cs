﻿using System.Collections.Generic;
using TachyonCommon.Hash;
using System;
using TachyonCommon;

namespace TachyonServerRPC
{
    public class EndPointMap
    {
        public readonly HostStub Stub;
        public readonly ISerializer Serializer;
        public readonly AskHasher Hasher = new AskHasher();

        private readonly List<Action<ClientConnection>> _sendEndPoints = new List<Action<ClientConnection>>();
        private readonly List<Action<ClientConnection>> _askEndPoints = new List<Action<ClientConnection>>();

        public EndPointMap(ISerializer serializer)
        {
            Serializer = serializer;
            Stub = new HostStub(serializer);
        }

        /// <summary>
        /// Register a function which may be called by the client.
        /// </summary>
        /// <typeparam name="T">Type of the method's argument.</typeparam>
        /// <param name="endPoint">The method called by the client.</param>
        public void AddSendEndpoint<T>(Action<ClientConnection, T> endPoint)
        {
            AddSendEndpoint(endPoint, endPoint.Method.Name);
        }

        public void AddSendEndpoint<T>(Action<ClientConnection, T> endPoint, string methodName)
        {
            _sendEndPoints.Add((client) =>
            {
                var method = new Action<T>((parameters) => endPoint.Invoke(client, parameters));
                var methodHash = Hasher.HashString(methodName);
                Stub.Register<T>(methodHash, method, client.Guid);
            });
        }

        /// <summary>
        /// Register a function which may be called by the client, to which the server will respond with data.
        /// </summary>
        /// <typeparam name="I">Request parameter type</typeparam>
        /// <typeparam name="O">Response data type</typeparam>
        /// <param name="clientMethod">Method which will handle the request.</param>
        public void AddAskEndpoint<I, O>(Func<ClientConnection, I, O> clientMethod)
        {
            AddAskEndpoint(clientMethod, clientMethod.Method.Name);
        }

        public void AddAskEndpoint<I, O>(Func<ClientConnection, I, O> clientMethod, string methodName)
        {
            _askEndPoints.Add((client) =>
            {
                var func = new Func<I, O>((parameters) => clientMethod.Invoke(client, parameters));
                var methodHash = Hasher.HashString(methodName);
                methodHash = AskHasher.SetAskBit(methodHash, true);
                Stub.Register<I, O>(methodHash, func, client.Guid);
            });
        }

        /// <summary>
        /// Register this connection to participate in endpoint interractions.
        /// </summary>
        /// <param name="connection">Connected to connect to endpoints.</param>
        public void RegisterConnection(ClientConnection connection)
        {
            foreach (var sendEndPoint in _sendEndPoints)
                sendEndPoint.Invoke(connection);

            foreach (var askEndpoint in _askEndPoints)
                askEndpoint.Invoke(connection);
        }
    }
}