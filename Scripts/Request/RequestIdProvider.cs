using System.Collections.Generic;
using Mirror;

namespace RequestForMirror
{
    public class RequestId
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public int ID;

        private RequestId(int id)
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
        private static readonly Dictionary<int, RequestId> RequestIdsPerClient = new Dictionary<int, RequestId>();

        public static readonly RequestId LocalRequestId = 0;

        static RequestIdProvider()
        {
            NetworkServer.OnConnectedEvent += OnConnectedEvent;
            NetworkServer.OnDisconnectedEvent += OnDisconnectedEvent;
        }

        private static void OnConnectedEvent(NetworkConnectionToClient conn)
        {
            RequestIdsPerClient.Add(conn.connectionId, 0);
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