using System.Diagnostics.CodeAnalysis;
using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    public abstract class Post<TReq, TRes> : RequestBase<TRes>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected TReq Request;
        
        [Client]
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        public void Send(TReq request, ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            InitSend(out var usingSerializerType, responseCallback, failCallback);
            
            if (usingSerializerType == RequestSerializerType.JsonUtility)
            {
                var json = JsonUtility.ToJson(request);
                CmdHandleRequestJson(json);
                return;
            }

            if (usingSerializerType == RequestSerializerType.MirrorBuiltIn)
            {
                CmdHandleRequestMirrorWeaver(request);
                return;
            }
        }

        [Server]
        private void CmdHandleRequestMirrorWeaver(TReq request)
        {
            Request = request;
            CmdHandleRequest();
        }

        [Server]
        private void CmdHandleRequestJson(string json)
        {
            var request = JsonUtility.FromJson<TReq>(json);
            Request = request;
            CmdHandleRequest();
        }
    }
    
    public abstract class Post<TReq, TReq2, TRes> : Post<TReq, TRes>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected TReq2 Request2;

        [Client]
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        public void Send(TReq request, TReq2 request2, ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            InitSend(out var usingSerializerType, responseCallback, failCallback);
            
            if (usingSerializerType == RequestSerializerType.JsonUtility)
            {
                var json = JsonUtility.ToJson(request);
                var json2 = JsonUtility.ToJson(request2);
                CmdHandleRequestJson(json, json2);
                return;
            }

            if (usingSerializerType == RequestSerializerType.MirrorBuiltIn)
            {
                CmdHandleRequestMirrorWeaver(request, request2);
                return;
            }
        }

        [Server]
        private void CmdHandleRequestMirrorWeaver(TReq request, TReq2 request2)
        {
            Request = request;
            Request2 = request2;
            CmdHandleRequest();
        }

        [Server]
        private void CmdHandleRequestJson(string json, string json2)
        {
            Request = JsonUtility.FromJson<TReq>(json);
            Request2 = JsonUtility.FromJson<TReq2>(json2);
            CmdHandleRequest();
        }
    }
}