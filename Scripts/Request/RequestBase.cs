using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mirror;
using RequestForMirror.Utils;
using UnityEngine;

namespace RequestForMirror
{
    public abstract class RequestBase<TRes> : NetworkBehaviour, IMarkedForCodeGen, IRequest 
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected Response<TRes> Response;
        
        // ReSharper disable once MemberCanBePrivate.Global
        protected NetworkConnectionToClient Sender; // server only
        private RequestId _requestId;               // server only

        protected Status Ok => new Status(true);
        protected Status Error => new Status(false);

        #region Client Actions

        public delegate void FailDelegate(string reason);
        public delegate void ResponseDelegate(TRes res);

        private class ResponseClientActions
        {
            public readonly ResponseDelegate OnResponse;
            public readonly FailDelegate OnFail;

            public ResponseClientActions(ResponseDelegate onResponse, FailDelegate onFail)
            {
                OnResponse = onResponse;
                OnFail = onFail;
            }
        }
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<RequestId, ResponseClientActions> _awaitingResponses = new Dictionary<RequestId, ResponseClientActions>();
        #endregion
        
        protected void InitSend(out RequestSerializerType usingSerializerType, ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            var requestId = RequestIdProvider.LocalId.Next();
            Debug.Log($"RequestID::: {requestId.ID}");
            _awaitingResponses.Add(
                requestId, 
                new ResponseClientActions(responseCallback, failCallback));
            
            var settings = SettingsUtility.Load<RequestSettings>();
            usingSerializerType = settings.serializationMethod;
        }
        
        [Server]
        protected virtual void CmdHandleRequest(NetworkConnectionToClient sender = null)
        {
            Response = new Response<TRes>();
            Sender = sender;
            _requestId = RequestIdProvider.GenerateId(Sender);
            OnRequest(out var status);
            SerializeAndRespond(status, sender);
        }

        [Server]
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        private void SerializeAndRespond(Status status, NetworkConnectionToClient sender = null)
        {
            var settings = SettingsUtility.Load<RequestSettings>();
            var serializerInUse = settings.serializationMethod;
            
            
            if (serializerInUse == RequestSerializerType.JsonUtility)
            {
                Debug.Log($"Payload: {Response.Payload}");
                var json = JsonUtility.ToJson(Response);
                Debug.Log($"JSON: {json}");
                TargetReceiveResponseJson(sender, _requestId.ID, status, json);
                return;
            }

            if (serializerInUse == RequestSerializerType.MirrorBuiltIn)
            {
                TargetReceiveResponseMirrorWeaver(sender, _requestId.ID, status, Response.Payload);
                return;
            }
        }
        
        [Client]
        // Deserialize using JsonUtility
        // ReSharper disable once UnusedParameter.Global
        protected virtual void TargetReceiveResponseJson(
            NetworkConnection target, 
            int id,
            Status status,
            string response)
        {
            var responseDeserialized = JsonUtility.FromJson<Response<TRes>>(response);
            Response.SetPayload(responseDeserialized.Payload);
            HandleResponse(id, status);
        }

        [Client]
        // Deserializes using Mirror's built-in serializer
        // ReSharper disable once UnusedParameter.Global
        protected virtual void TargetReceiveResponseMirrorWeaver(
            NetworkConnection target,
            int id,
            Status status,
            TRes response)
        {
            Response.SetPayload(response);
            HandleResponse(id, status);
        }

        [Client]
        private void HandleResponse(int id, Status status)
        {
            foreach (var key in _awaitingResponses.Keys)
            {
                Debug.Log(key);
            }
            if (!_awaitingResponses.ContainsKey(id))
            {
                Debug.LogError($"{GetType().Name}: callback with id {id} not found. Callbacks won't trigger");
                return;
            }

            var actions = _awaitingResponses[id];
            
            if (!status.RequestFailed)
                actions.OnResponse?.Invoke(Response.Payload);
            else
                actions.OnFail?.Invoke(status.Message);

            _awaitingResponses.Remove(id);
        }
        
        
        protected abstract void OnRequest(out Status status);
    }
    
    public class Response<TRes>
    {
        public TRes Payload;

        public void SetPayload(TRes data)
        {
            Payload = data;
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Status
    {
        public readonly bool RequestFailed;
        public string Message;

        public Status(bool ok, string message = null)
        {
            RequestFailed = !ok;
            Message = message;
        }

        // Required by Mirror's serializer
        public Status()
        {
        }

        public Status SetMessage(string message)
        {
            Message = message;
            return this;
        }
    }
}