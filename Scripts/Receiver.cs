// ReSharper disable once RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mirror;
using TwistCore;
using UnityEngine;
using Object = System.Object;

namespace RequestForMirror
{
    public abstract class Receiver : NetworkBehaviour
    {
        public static NetworkIdentity GlobalRequestManager = null;

        protected bool HasAdjacentRequestManager = false;
        protected RequestManagerBase RequestManager;
        private static readonly Dictionary<int, Receiver> ReceiversByConnId = new Dictionary<int, Receiver>();
        private static Receiver _localReceiver;

        private static Action _onLocalReceiverReadyOnce;

        [Client]
        private void OnEnable()
        {
            if (isServerOnly) return;
            RequestManager = GetComponent<RequestManagerBase>();
            _settings = SettingsUtility.Load<RequestSettings>();
            HasAdjacentRequestManager = RequestManager != null;
            HandleLocalReceiverReady(this);
        }

        private static void HandleLocalReceiverReady(Receiver receiver)
        {
            _localReceiver = receiver;

            var queuedRequestsCount = _onLocalReceiverReadyOnce?.GetInvocationList().Length ?? 0;
            if (queuedRequestsCount <= 0) return;
            Debug.Log($"Sending {queuedRequestsCount} queued requests");
            _onLocalReceiverReadyOnce?.Invoke();
            _onLocalReceiverReadyOnce = null;
        }

        private static Receiver GetCachedReceiver(int clientConnId, bool isServer = true)
        {
            Receiver receiver = null;
            var who = isServer ? "[Server]" : "[Client]";

            if (!isServer && _localReceiver != null)
            {
                return _localReceiver;
            }
            else if (isServer &&
                     (ReceiversByConnId.ContainsKey(clientConnId) && ReceiversByConnId[clientConnId] != null))
            {
                return ReceiversByConnId[clientConnId];
            }
            else
            {
                var ply = isServer ? NetworkServer.connections[clientConnId] : NetworkClient.connection;
                foreach (var networkIdentity in ply.owned)
                {
                    receiver = networkIdentity.GetComponent<Receiver>();
                    if (receiver == null) continue;
                    Debug.Log(who + " Caching receiver connid " + clientConnId);

                    if (isServer) ReceiversByConnId[clientConnId] = receiver;
                    else _localReceiver = receiver;

                    break;
                }
            }

            if (receiver == null)
            {
                Debug.LogWarning(who + " Receiver not found, connid " + clientConnId);
            }

            return receiver;
        }

        private static Receiver GetCachedReceiverClient()
        {
            return GetCachedReceiver(-1, false);
        }

        private IRequest FindAwaitingResponse(IEnumerable<IRequest> requests, int requestId)
        {
            return requests?.FirstOrDefault(r => r.IsAwaitingResponse(requestId));
        }

        public IRequest FindAwaitingResponse(int requestId)
        {
            Debugg.Log(string.Join(", ", RequestManagerBase.Global.GetComponents<IRequest>().Select(c => c.GetType().Name)));
            return FindAwaitingResponse(GetComponents<IRequest>(), requestId) ??
                   // ReSharper disable once Unity.NoNullPropagation
                   FindAwaitingResponse(RequestManagerBase.Global.GetComponents<IRequest>(), requestId);
        }

        private static readonly Dictionary<Type, MethodInfo> ResponseMethods = new Dictionary<Type, MethodInfo>();

        private static readonly Type[] ResponseMethodParamTypes =  
            {
                typeof(NetworkConnection), 
                typeof(int), 
                typeof(Status), 
                null
            };
        
        private static MethodInfo GetCachedResponseMethod(string methodName, Type receiverType, Type responseType)
        {
            if (ResponseMethods.TryGetValue(responseType, out var foundMethod))
                return foundMethod;
            
            const int responseTypeIndex = 3;
            ResponseMethodParamTypes[responseTypeIndex] = responseType;
            var method = receiverType.GetRuntimeMethod(methodName, ResponseMethodParamTypes);
            if (method != null) ResponseMethods[responseType] = method;
            return method;
        }
        
        private static RequestSettings _settings;
        //private static RequestSettings Settings => _settings ??= SettingsUtility.Load<RequestSettings>();

        public static void SendResponse<TRes>(NetworkConnection target,
            int requestID,
            Status status,
            TRes response)
        {
            var receiver = GetCachedReceiver(target.connectionId);
            Debugg.Log("Receiver " + receiver);
            Debugg.Log($"[Server] Sending response for request ID {requestID}");
            
            
            var tReceiver = receiver.GetType();
            MethodInfo method;

            if (_settings.cacheMethodInfo)
            {
                method = GetCachedResponseMethod("TargetReceiveResponseMirrorWeaver", tReceiver, typeof(TRes));
            }
            else
            {
                var paramTypes =  
                    new[]
                    {
                        typeof(NetworkConnection), 
                        typeof(int), 
                        typeof(Status), 
                        typeof(TRes)
                    };
                //GetRuntimeMethod
                method = tReceiver.GetRuntimeMethod("TargetReceiveResponseMirrorWeaver", paramTypes);
            }
            
            Debugg.Log(method);

            object[] parameters =
            {
                target, requestID, status, response
            };
            method?.Invoke(receiver, parameters);
        }

        
        public static void SendRequest<TRes>(RequestBase<TRes> request)
        {
            var receiver = GetCachedReceiverClient();
            if (receiver == null)
            {
                _onLocalReceiverReadyOnce += () => SendRequest(request);
                Debugg.Log($"Queued request {request.GetType().Name} because local receiver is not ready yet");
                return;
            }
            var receiverType = receiver.GetType();
            
            var requestType = request.GetType();
            var requestName = requestType.Name.Split('`').FirstOrDefault();

            var tReqTypes = requestType.BaseType!.GenericTypeArguments;
            tReqTypes = tReqTypes.Take(tReqTypes.Length - 1).ToArray();
            
            var paramTypes = tReqTypes.Concat(new[] {typeof(NetworkConnectionToClient)});
            //GetRuntimeMethod
            var method = receiverType.GetMethod($"CmdHandleRequest_{requestName}", paramTypes.ToArray());

            var paramValues = new List<object>();
            
            for (var i = 0; i < tReqTypes.Length; i++)
            {
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                var fieldName = "Request";
                if (i > 0) fieldName += i + 1; //Request2, Request3...
                var field = requestType.GetField(fieldName, bindingFlags);
                if (field == null)
                {
                    Debugg.Log($"GetField() for field {fieldName} of class {requestType.Name} did return null");
                    continue;
                }
                paramValues.Add(field.GetValue(request));
            }

            paramValues.Add(null); //null value for NetworkConnectionToClient will be filled up by Mirror
            method.Invoke(receiver, paramValues.ToArray());
        }

        [UsedImplicitly]
        protected void PushResponseOnClient<TRes>(NetworkConnection target,
            int requestID,
            Status status,
            TRes response)
        {
            Debugg.Log($"[Client] Received response for request ID {requestID} - {response.GetType().Name} - {response}");
            var requestHandler = FindAwaitingResponse(requestID);
            if (requestHandler == null)
            {
                //todo: log possible losses
                return;
            }
            var request = (RequestBase<TRes>)requestHandler;
            Debugg.Log(request);
            request.TargetReceiveResponseMirrorWeaver(target, requestID, status, response);
        }

        [UsedImplicitly]
        protected void PushRequestOnServer(Type requestType, params object[] args)
        {
            if (HasAdjacentRequestManager) RequestManager.Dispatch(requestType, args);
            else RequestManagerBase.Global.Dispatch(requestType, args);
        }
    }
}