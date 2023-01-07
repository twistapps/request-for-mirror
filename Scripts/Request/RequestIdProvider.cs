using System.Collections.Generic;
using Mirror;

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
        
        public static implicit operator RequestId(int n) => new RequestId(n);
        public static explicit operator int(RequestId request) => request.ID;
    }
    
    public static class RequestIdProvider
    {
        private static readonly Dictionary<int, RequestId> RequestIdsPerClient = new Dictionary<int, RequestId>(); //ids stored on server
        public static readonly RequestId localId = new RequestId(0); //id stored in client

        static RequestIdProvider()
        {
            NetworkServer.OnConnectedEvent += OnConnectedEvent;
            NetworkServer.OnDisconnectedEvent += OnDisconnectedEvent;
        }

        public static void RegisterHostConnection()
        {
            const int hostConnectionId = 0;
            if (!RequestIdsPerClient.ContainsKey(hostConnectionId))
                RequestIdsPerClient.Add(hostConnectionId,0);
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