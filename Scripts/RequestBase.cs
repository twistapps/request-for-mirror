#if MIRROR
using Mirror;
#elif UNITY_NETCODE
using Unity.Netcode;
#endif
using System;
using System.Collections.Generic;
using Modula;
using UnityEngine;

namespace RequestForMirror
{
    #if REQUESTIFY_ENABLED
    public abstract class RequestBase<TRes> : MonoBehaviour, IRequest
    {
        private RequestId _requestId; // server only

        // ReSharper disable once MemberCanBePrivate.Global
        protected Response<TRes> Response;

        #if MIRROR
        // ReSharper disable once MemberCanBePrivate.Global
        protected NetworkConnectionToClient Sender; // server only
        #elif UNITY_NETCODE
        protected NetworkConnectionToClient Sender;
        private readonly Dictionary<ulong, ClientRpcParams> _clientIdsToSendParams =
            new Dictionary<ulong, ClientRpcParams>();
        #endif

        protected Status Ok => new Status(true);
        protected Status Error => new Status(false);

        public Type ResponseType => typeof(TRes);

        public bool IsAwaitingResponse(int requestId)
        {
            return _awaitingResponse.ContainsKey(requestId);
        }

        public void HandleRequest(object[] args)
        {
            #if MIRROR
            Sender = (NetworkConnectionToClient)args[args.Length - 1];
            #elif UNITY_NETCODE
            Sender = new NetworkConnectionToClient(((ServerRpcParams)args[args.Length - 1]).Receive.SenderClientId);
            #endif
            Response = new Response<TRes>();
            HandleRequestArgs(args);
            _requestId = RequestIdProvider.GenerateId(Sender.connectionId);

            // var senderConnectionId =
            //     #if MIRROR
            //     Sender?.connectionId;
            //     DebugLevels.Log($"[Server] Generated request Id: {_requestId.ID}; sender: {senderConnectionId}");
            //     #elif UNITY_NETCODE
            //     Sender;
            // #endif


            DebugLevels.Log($"[Server] Generated request Id: {_requestId.ID}; sender: {Sender.connectionId}");
            var status = OnRequest();

            #if UNITY_NETCODE
            ClientRpcParams sendParams;
            if (!_clientIdsToSendParams.TryGetValue(Sender.connectionId, out sendParams))
            {
                sendParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { Sender.connectionId }
                    }
                };
                _clientIdsToSendParams[Sender.connectionId] = sendParams;
            }
            #endif

            #if MIRROR
            Receiver.SendResponse(Sender, _requestId.ID, status, Response.payload);
            #elif UNITY_NETCODE
            if (status.IsBroadcast)
                status.requestType = Convert.ToUInt16(RequestManagerBase.Global.attachments.IndexOf(this as IModule));

            Receiver.SendResponse(sendParams, _requestId.ID, status, Response.payload);
            #endif
        }

        protected abstract void HandleRequestArgs(object[] args);

        protected void RegisterResponseCallbacks(ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            var requestId = RequestIdProvider.LocalID.Next();
            DebugLevels.Log($"[Client] Preparing to send RequestID:::{requestId.ID}");
            _awaitingResponse.Add(
                requestId.ID,
                new ResponseClientActions(responseCallback, failCallback));
        }

        //[Client]
        // Deserializes using Mirror's built-in serializer
        // ReSharper disable once UnusedParameter.Global
        public virtual void OnReceiveResponse(
            #if MIRROR
            NetworkConnection target,
            #elif UNITY_NETCODE
            ClientRpcParams clientRpcParams,
            #endif
            int id,
            Status status,
            TRes response
        )
        {
            Response = new Response<TRes>();
            if (response != null)
                Response.SetPayload(response);
            HandleResponse(id, status);
        }

        #if MIRROR
        [Client]
        #endif
        private void HandleResponse(int id, Status status)
        {
            var keys = string.Join(", ", _awaitingResponse.Keys);
            DebugLevels.Log("Awaiting response keys: " + keys);

            if (!_awaitingResponse.ContainsKey(id))
            {
                DebugLevels.LogError($"{GetType().Name}: callback with id {id} not found. Callbacks won't trigger");
                return;
            }

            var actions = _awaitingResponse[id];

            if (!status.RequestFailed)
                actions.OnResponse?.Invoke(Response.payload);
            else
                actions.OnFail?.Invoke(status.Message);

            _awaitingResponse.Remove(id);
        }


        //todo: change to 'abstract Status OnRequest()'
        protected abstract Status OnRequest();

        #region Client Actions

        public delegate void FailDelegate(string reason);

        public delegate void ResponseDelegate(TRes res);

        public ResponseDelegate BroadcastHandler;
        public FailDelegate BroadcastFailHandler;

        private class ResponseClientActions
        {
            public readonly FailDelegate OnFail;
            public readonly ResponseDelegate OnResponse;

            public ResponseClientActions(ResponseDelegate onResponse, FailDelegate onFail)
            {
                OnResponse = onResponse;
                OnFail = onFail;
            }
        }

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<int, ResponseClientActions> _awaitingResponse =
            new Dictionary<int, ResponseClientActions>();

        #endregion
    }

    // public abstract class RequestBase<TRes, TBroadcast> : RequestBase<TRes>
    // {
    //     public delegate void BroadcastDelegate(TBroadcast res);
    //     public BroadcastDelegate BroadcastResponseHandler;
    //     public FailDelegate BroadcastFailHandler;
    // }
    #endif
}