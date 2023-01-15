using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    public class RequestId
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public int ID;

        internal RequestId(int id)
        {
            ID = id;
        }

        public RequestId Next()
        {
            ID++;
            return new RequestId(ID);
        }

        public static implicit operator RequestId(int n)
        {
            return new RequestId(n);
        }

        public static explicit operator int(RequestId request)
        {
            return request.ID;
        }
    }

    public static class RequestIdProvider
    {
        private static readonly Dictionary<int, RequestId>
            RequestIdsPerClient = new Dictionary<int, RequestId>(); //ids stored on server

        public static readonly RequestId localId = new RequestId(0); //id stored in client

        static RequestIdProvider()
        {
            Debug.Log("RequestIdProvider constructor");
            NetworkServer.OnConnectedEvent += OnConnectedEvent;
            NetworkServer.OnDisconnectedEvent += OnDisconnectedEvent;
            NetworkClient.OnConnectedEvent += RegisterHostConnection;

            //RegisterHostConnection();
            RegisterAlreadyConnected();
        }

        private static void RegisterAlreadyConnected()
        {
            foreach (var client in NetworkServer.connections)
                if (!RequestIdsPerClient.ContainsKey(client.Value.connectionId))
                    RequestIdsPerClient[client.Value.connectionId] = 0;
        }

        public static void RegisterHostConnection()
        {
            if (!NetworkClient.activeHost) return;
            const int hostConnectionId = 0;
            if (!RequestIdsPerClient.ContainsKey(hostConnectionId))
                RequestIdsPerClient.Add(hostConnectionId, 0);
            Debug.Log("Registered Host Connection");
        }

        private static void OnConnectedEvent(NetworkConnectionToClient conn)
        {
            RequestIdsPerClient[conn.connectionId] = 0;
        }

        private static void OnDisconnectedEvent(NetworkConnectionToClient conn)
        {
            RequestIdsPerClient.Remove(conn.connectionId);
        }

        public static RequestId GenerateId(NetworkConnection conn)
        {
            var id = RequestIdsPerClient[conn.connectionId].Next();
            return id;
        }
    }
}