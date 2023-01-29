#if MIRROR
using Mirror;
#elif UNITY_NETCODE
using Unity.Netcode;
#endif
using System.Collections.Generic;
using UnityEngine;

#if REQUESTIFY_ENABLED
namespace RequestForMirror
{
    public static class RequestIdProvider
    {
        #if MIRROR
        private static readonly Dictionary<int, RequestId>
            RequestIdsPerClient = new Dictionary<int, RequestId>(); //ids stored on server

        #elif UNITY_NETCODE

        private static readonly Dictionary<ulong, RequestId>
            RequestIdsPerClient = new Dictionary<ulong, RequestId>(); //ids stored on server

        #endif

        public static RequestId LocalID = new RequestId(0); //id stored in client

        static RequestIdProvider()
        {
            Debug.Log("RequestIdProvider constructor");


            #if UNITY_NETCODE
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.OnClientConnectedCallback += obj => LocalID = new RequestId(0);

            if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnConnectedEvent;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedEvent;
            }
            #elif MIRROR
            NetworkServer.OnConnectedEvent += OnConnectedEvent;
            NetworkServer.OnDisconnectedEvent += OnDisconnectedEvent;
            NetworkClient.OnConnectedEvent += () => LocalID = new RequestId(0);
            #endif

            NetworkEvents.ExecuteWhenStartedHost(RegisterHostConnection);
            RegisterAlreadyConnected();
        }

        private static void RegisterAlreadyConnected()
        {
            #if MIRROR
            foreach (var client in NetworkServer.connections)
                if (!RequestIdsPerClient.ContainsKey(client.Value.connectionId))
                        RequestIdsPerClient[client.Value.connectionId] = 0;

            #elif UNITY_NETCODE

            if (!NetworkManager.Singleton.IsServer) return;

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                if (!RequestIdsPerClient.ContainsKey(clientId))
                    RequestIdsPerClient[clientId] = 0;

            #endif
        }

        public static void RegisterHostConnection()
        {
            #if MIRROR
            if (!NetworkClient.activeHost) return;
            const int hostConnectionId = 0;

            #elif UNITY_NETCODE

            if (!NetworkManager.Singleton.IsHost) return;
            var hostConnectionId = NetworkManager.Singleton.LocalClientId;

            #endif

            if (!RequestIdsPerClient.ContainsKey(hostConnectionId))
                RequestIdsPerClient.Add(hostConnectionId, 0);
            Debug.Log("Registered Host Connection");
        }

        private static void OnConnectedEvent(
            #if MIRROR
            NetworkConnectionToClient conn
            #elif UNITY_NETCODE
            ulong conn
            #endif
        )
        {
            RequestIdsPerClient[conn
                #if MIRROR
                .connectionId
                #endif
            ] = 0;
        }

        private static void OnDisconnectedEvent(
            #if MIRROR
            NetworkConnectionToClient conn
            #elif UNITY_NETCODE
            ulong conn
            #endif
        )
        {
            RequestIdsPerClient.Remove(conn
                #if MIRROR
                .connectionId
                #endif
            );
        }

        public static RequestId GenerateId(
            #if MIRROR
            NetworkConnection conn
            #elif UNITY_NETCODE
            ulong conn
            #endif
        )
        {
            var id = RequestIdsPerClient[conn
                #if MIRROR
                .connectionId
                #endif
            ].Next();

            return id;
        }
    }
}
#endif