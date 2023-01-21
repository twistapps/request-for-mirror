using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    public abstract class RequestBase<TRes> : MonoBehaviour, IRequest
    {
        private RequestId _requestId; // server only

        // ReSharper disable once MemberCanBePrivate.Global
        protected Response<TRes> Response;

        // ReSharper disable once MemberCanBePrivate.Global
        protected NetworkConnectionToClient Sender; // server only

        protected Status Ok => new Status(true);
        protected Status Error => new Status(false);

        public Type ResponseType => typeof(TRes);

        public bool IsAwaitingResponse(int requestId)
        {
            return _awaitingResponse.ContainsKey(requestId);
        }

        public void HandleRequest(object[] args)
        {
            Sender = (NetworkConnectionToClient)args[args.Length - 1];
            Response = new Response<TRes>();
            HandleRequestArgs(args);
            _requestId = RequestIdProvider.GenerateId(Sender);
            Debugg.Log($"[Server] Generated request Id: {_requestId.ID}; sender: {Sender?.connectionId}");
            var status = OnRequest();
            Receiver.SendResponse(Sender, _requestId.ID, status, Response.payload);
        }

        protected abstract void HandleRequestArgs(object[] args);

        protected void RegisterResponseCallbacks(ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            var requestId = RequestIdProvider.LocalID.Next();
            Debugg.Log($"[Client] Preparing to send RequestID:::{requestId.ID}");
            _awaitingResponse.Add(
                requestId.ID,
                new ResponseClientActions(responseCallback, failCallback));
        }

        //[Client]
        // Deserializes using Mirror's built-in serializer
        // ReSharper disable once UnusedParameter.Global
        public virtual void TargetReceiveResponseMirrorWeaver(
            NetworkConnection target,
            int id,
            Status status,
            TRes response)
        {
            Response = new Response<TRes>();
            if (response != null)
                Response.SetPayload(response);
            HandleResponse(id, status);
        }

        [Client]
        private void HandleResponse(int id, Status status)
        {
            var keys = string.Join(", ", _awaitingResponse.Keys);
            Debugg.Log("Awaiting response keys: " + keys);

            if (!_awaitingResponse.ContainsKey(id))
            {
                Debugg.LogError($"{GetType().Name}: callback with id {id} not found. Callbacks won't trigger");
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
}