using System.Diagnostics.CodeAnalysis;
using Mirror;
using RequestForMirror.Utils;
using UnityEngine;

namespace RequestForMirror
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class Fetch<TRes> : NetworkBehaviour, IMarkedForCodeGen, IRequest
    {
        public delegate void FailDelegate(string reason);
        public delegate void ResponseDelegate(TRes res);

        private FailDelegate _onFail;
        private ResponseDelegate _onResponse;

        // ReSharper disable once MemberCanBePrivate.Global
        protected Response<TRes> Response;

        // ReSharper disable once MemberCanBePrivate.Global
        protected NetworkConnectionToClient Sender; // server only
        
        public Status Ok => new Status(true);
        public Status Error => new Status(false);
        
        protected void InitSend(out RequestSerializerType usingSerializerType, ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            _onResponse = responseCallback;
            _onFail = failCallback;
            
            var settings = SettingsUtility.Load<RequestSettings>();
            usingSerializerType = settings.serializationMethod;
        }

        [Client]
        public void Send(ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            InitSend(out _, responseCallback, failCallback);
            CmdHandleRequest();
        }

        [Server]
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        protected virtual void CmdHandleRequest(NetworkConnectionToClient sender = null)
        {
            Response = new Response<TRes>();
            Sender = sender;
            OnRequest(out var status);

            var settings = SettingsUtility.Load<RequestSettings>();
            var serializerInUse = settings.serializationMethod;
            
            
            if (serializerInUse == RequestSerializerType.JsonUtility)
            {
                var json = JsonUtility.ToJson(Response.Payload);
                TargetReceiveResponseJson(sender, status, json);
                return;
            }

            if (serializerInUse == RequestSerializerType.MirrorBuiltIn)
            {
                TargetReceiveResponseMirrorWeaver(sender, status, Response.Payload);
                return;
            }
        }

        [Client]
        // Deserialize using JsonUtility
        // ReSharper disable once UnusedParameter.Global
        protected virtual void TargetReceiveResponseJson(NetworkConnection target, Status status,
            string response)
        {
            var responseDeserialized = JsonUtility.FromJson<TRes>(response);
            Response.SetPayload(responseDeserialized);
            HandleResponse(status);
        }

        [Client]
        // Deserializes using Mirror's built-in serializer
        // ReSharper disable once UnusedParameter.Global
        protected virtual void TargetReceiveResponseMirrorWeaver(NetworkConnection target, Status status,
            TRes response)
        {
            Response.SetPayload(response);
            HandleResponse(status);
        }

        [Client]
        private void HandleResponse(Status status)
        {
            if (!status.RequestFailed)
            {
                _onResponse?.Invoke(Response.Payload);
                return;
            }


            _onFail?.Invoke(status.Message);
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