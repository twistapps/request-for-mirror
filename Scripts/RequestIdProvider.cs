using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    public static class RequestIdProvider
    {
        private static readonly Dictionary<int, RequestId>
            RequestIdsPerClient = new Dictionary<int, RequestId>(); //ids stored on server

        public static RequestId LocalID = new RequestId(0); //id stored in client

        static RequestIdProvider()
        {
            Debug.Log("RequestIdProvider constructor");
            NetworkServer.OnConnectedEvent += OnConnectedEvent;
            NetworkServer.OnDisconnectedEvent += OnDisconnectedEvent;

            NetworkClient.OnConnectedEvent += () => LocalID = new RequestId(0);
            ;

            NetworkManagerWithEvents.ExecuteWhenStartedHost(RegisterHostConnection);
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