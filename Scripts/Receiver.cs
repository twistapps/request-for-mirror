// ReSharper disable once RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TwistCore;
using UnityEngine;
using Object = System.Object;

#if MIRROR
using Mirror;
#elif UNITY_NETCODE
using Unity.Netcode;
#endif


#if UNITY_NETCODE
public class NetworkConnection
{
    // ReSharper disable once InconsistentNaming
    public readonly ulong connectionId;

    public NetworkConnection(ulong connectionId)
    {
        this.connectionId = connectionId;
    }

    public NetworkObject identity => NetworkManager.Singleton.ConnectedClients
        .FirstOrDefault(c => c.Key == connectionId).Value.PlayerObject;

    public override bool Equals(object obj)
    {
        return connectionId.Equals(obj);
    }

    protected bool Equals(NetworkConnection other)
    {
        return connectionId == other.connectionId;
    }

    public override int GetHashCode()
    {
        return connectionId.GetHashCode();
    }

    public static explicit operator ulong(NetworkConnection src)
    {
        return src.connectionId;
    }
}

public class NetworkConnectionToClient : NetworkConnection
{
    public NetworkConnectionToClient(ulong connectionId) : base(connectionId)
    {
    }
}
#endif

namespace RequestForMirror
{
    #if REQUESTIFY_ENABLED
    public abstract class Receiver : NetworkBehaviour
    {
        //public static NetworkIdentity GlobalRequestManager = null;
        #if MIRROR
        private static readonly Dictionary<int, Receiver> ReceiversByConnId = new Dictionary<int, Receiver>();
        private const int localConnectionId = 0;
        #elif UNITY_NETCODE
        private static readonly Dictionary<ulong, Receiver> ReceiversByConnId = new Dictionary<ulong, Receiver>();
        private static ulong localConnectionId => NetworkManager.Singleton.LocalClientId;
        #endif
        private static Receiver _localReceiver;

        private static Action _onLocalReceiverReadyOnce;

        private static readonly Dictionary<Type, MethodInfo> ResponseMethods = new Dictionary<Type, MethodInfo>();

        #if MIRROR
        private static readonly Type[] ResponseMethodParamTypes =
        {
            typeof(NetworkConnection),
            typeof(int),
            typeof(Status),
            null
        };
        private const int ResponseTypeIndex = 3;
        #elif UNITY_NETCODE
        private static readonly Type[] ResponseMethodParamTypes =
        {
            typeof(int),
            typeof(Status),
            null,
            typeof(ClientRpcParams)
        };

        private const int ResponseTypeIndex = 2;
        #endif

        private static RequestSettings _settings;

        protected bool HasAdjacentRequestManager;
        protected RequestManagerBase RequestManager;

        #if MIRROR
        [Client]
        #endif
        private void OnEnable()
        {
            #if MIRROR
            if (isServerOnly) return;
            #elif UNITY_NETCODE
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient) return;
            Debug.Log(GetComponent<NetworkObject>().IsOwnedByServer);
            #endif
            RequestManager = GetComponent<RequestManagerBase>();
            _settings = SettingsUtility.Load<RequestSettings>();
            HasAdjacentRequestManager = RequestManager != null;
            OnLocalReceiverReady(this);
        }

        private static void OnLocalReceiverReady(Receiver receiver)
        {
            _localReceiver = receiver;

            var queuedRequestsCount = _onLocalReceiverReadyOnce?.GetInvocationList().Length ?? 0;
            if (queuedRequestsCount <= 0) return;
            Debug.Log($"Sending {queuedRequestsCount} queued requests");
            _onLocalReceiverReadyOnce?.Invoke();
            _onLocalReceiverReadyOnce = null;
        }

        private static Receiver GetCachedReceiver(
            #if MIRROR
            int clientConnId,
            #elif UNITY_NETCODE
            ulong clientConnId,
            #endif
            bool isServer = true)
        {
            Receiver receiver = null;
            var who = isServer ? "[Server]" : "[Client]";

            switch (isServer)
            {
                case false when _localReceiver != null:
                    return _localReceiver;
                case true when ReceiversByConnId.TryGetValue(clientConnId, out receiver):
                    return receiver;
            }
            #if MIRROR
            var connection = isServer ? NetworkServer.connections[clientConnId] : NetworkClient.connection;
            #elif UNITY_NETCODE
            var connection = isServer
                ? NetworkManager.Singleton.ConnectedClients[clientConnId]
                : NetworkManager.Singleton.ConnectedClients[localConnectionId];
            #endif
            foreach (var networkIdentity in connection
                         #if MIRROR
                         .owned
                         #elif UNITY_NETCODE
                         .OwnedObjects
                     #endif
                    )
            {
                receiver = networkIdentity.GetComponent<Receiver>();
                if (receiver == null) continue;
                Debug.Log(who + " Caching receiver connid " + clientConnId);

                if (isServer) ReceiversByConnId[clientConnId] = receiver;
                else _localReceiver = receiver;

                break;
            }

            if (receiver == null) Debug.LogWarning(who + " Receiver not found, connid " + clientConnId);

            return receiver;
        }

        private static Receiver GetCachedReceiverClient()
        {
            #if MIRROR
            return GetCachedReceiver(0, false);
            #elif UNITY_NETCODE
            var clientId = NetworkManager.Singleton.LocalClientId;
            return GetCachedReceiver(clientId, false);
            #endif
        }

        private static IRequest FindAwaitingResponse(IEnumerable<IRequest> requests, int requestId)
        {
            return requests?.FirstOrDefault(r => r.IsAwaitingResponse(requestId));
        }

        public IRequest FindAwaitingResponse(int requestId)
        {
            //DebugLevels.Log(string.Join(", ",
            //    RequestManagerBase.Global.GetComponents<IRequest>().Select(c => c.GetType().Name)));
            return FindAwaitingResponse(GetComponents<IRequest>(), requestId) ??
                   // ReSharper disable once Unity.NoNullPropagation
                   FindAwaitingResponse(RequestManagerBase.Global.GetComponents<IRequest>(), requestId);
        }

        private static MethodInfo GetCachedResponseMethod(string methodName, Type receiverType, Type responseType)
        {
            if (ResponseMethods.TryGetValue(responseType, out var foundMethod))
                return foundMethod;


            ResponseMethodParamTypes[ResponseTypeIndex] = responseType;
            var method = receiverType.GetRuntimeMethod(methodName, ResponseMethodParamTypes);
            if (method != null) ResponseMethods[responseType] = method;
            return method;
        }
        //private static RequestSettings Settings => _settings ??= SettingsUtility.Load<RequestSettings>();

        private static readonly ClientRpcParams ParamsBroadcast = new ClientRpcParams
        {
            Send = new ClientRpcSendParams()
        };

        public static void SendResponse<TRes>(
            #if MIRROR
            NetworkConnection target,
            #elif UNITY_NETCODE
            ClientRpcParams target,
            #endif
            int requestID,
            Status status,
            TRes response)
        {
            var receiver = GetCachedReceiver(target
                    #if MIRROR
                .connectionId
                    #elif UNITY_NETCODE
                    .Send.TargetClientIds[0]
                #endif
            );
            DebugLevels.Log("Receiver " + receiver);
            DebugLevels.Log($"[Server] Sending response for request ID {requestID}");


            var tReceiver = receiver.GetType();
            MethodInfo method;

            if (_settings.cacheMethodInfo)
            {
                method = GetCachedResponseMethod("TargetReceiveResponseClientRpc", tReceiver, typeof(TRes));
            }
            else
            {
                var paramTypes =
                    new[]
                    {
                        #if MIRROR
                        typeof(NetworkConnection),
                        #endif
                        typeof(int),
                        typeof(Status),
                        typeof(TRes)
                        #if UNITY_NETCODE
                        , typeof(ClientRpcParams),
                        #endif
                    };
                //GetRuntimeMethod
                method = tReceiver.GetRuntimeMethod("TargetReceiveResponseClientRpc", paramTypes);
            }

            DebugLevels.Log(method);

            #if UNITY_NETCODE
            if (status.IsBroadcast) target = ParamsBroadcast;
            #endif


            #if MIRROR
            object[] parameters = { target, requestID, status, response };
            #elif UNITY_NETCODE
            object[] parameters = { requestID, status, response, target };
            #endif

            method?.Invoke(receiver, parameters);
        }


        public static void SendRequest<TRes>(RequestBase<TRes> request)
        {
            var receiver = GetCachedReceiverClient();
            if (receiver == null)
            {
                _onLocalReceiverReadyOnce += () => SendRequest(request);
                DebugLevels.Log($"Queued request {request.GetType().Name} because local receiver is not ready yet");
                return;
            }

            var receiverType = receiver.GetType();

            var requestType = request.GetType();
            var requestName = requestType.Name.Split('`').FirstOrDefault();

            var tReqTypes = requestType.BaseType!.GenericTypeArguments;
            tReqTypes = tReqTypes.Take(tReqTypes.Length - 1).ToArray();

            #if MIRROR
            var paramTypes = tReqTypes.Concat(new[] { typeof(NetworkConnectionToClient) });
            #elif UNITY_NETCODE
            var paramTypes = tReqTypes.Concat(new[] { typeof(ServerRpcParams) });
            #endif
            //GetRuntimeMethod
            var method = receiverType.GetMethod($"CmdHandleRequest_{requestName}_ServerRpc", paramTypes.ToArray());

            var paramValues = new List<object>();

            for (var i = 0; i < tReqTypes.Length; i++)
            {
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

                var fieldName = "Request";
                if (i > 0) fieldName += i + 1; //Request2, Request3...

                var field = requestType.GetField(fieldName, bindingFlags);
                if (field == null)
                {
                    DebugLevels.Log($"GetField() for field {fieldName} of class {requestType.Name} did return null");
                    continue;
                }

                var requestPayload = field.GetValue(request);
                paramValues.Add(requestPayload);
            }

            paramValues.Add(null); //null value for NetworkConnectionToClient will be filled up by Mirror
            method.Invoke(receiver, paramValues.ToArray());
        }

        [UsedImplicitly]
        protected void PushResponseOnClient<TRes>(
            #if MIRROR
            NetworkConnection target,
            #elif UNITY_NETCODE
            //todo: watch args order
            ClientRpcParams clientRpcParams,
            #endif
            int requestID,
            Status status,
            TRes response
        )
        {
            DebugLevels.Log(
                $"[Client] Received response for request ID {requestID} - {response.GetType().Name} - {response} {status.Message} ");
            var requestHandler = FindAwaitingResponse(requestID);
            if (requestHandler == null)
            {
                if (!IsLocalPlayer && status.IsBroadcast &&
                    RequestManagerBase.Global.attachments.Count > status.requestType)
                {
                    Debug.Log(status.requestType);
                    var broadcastHandler = (RequestBase<TRes>)RequestManagerBase.Global.attachments[status.requestType];
                    if (status.RequestFailed)
                        broadcastHandler.BroadcastFailHandler?.Invoke(status.Message);
                    else
                        broadcastHandler.BroadcastHandler?.Invoke(response);
                }

                return;
            }

            var request = (RequestBase<TRes>)requestHandler;
            DebugLevels.Log(request);

            request.OnReceiveResponse(
                #if MIRROR
                target,
                #elif UNITY_NETCODE
                clientRpcParams,
                #endif
                requestID,
                status,
                response
            );
        }

        [UsedImplicitly]
        protected void PushRequestOnServer(Type requestType, params object[] args)
        {
            if (HasAdjacentRequestManager) RequestManager.Dispatch(requestType, args);
            else RequestManagerBase.Global.Dispatch(requestType, args);
        }
    }
    #endif
}