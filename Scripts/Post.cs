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
            Request = request;
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
            // if (usingSerializerType == RequestSerializerType.JsonUtility)
            // {
            //     var json = JsonUtility.ToJson(request);
            //     CmdHandleRequestJson(json);
            //     return;
            // }
            //
            // if (usingSerializerType == RequestSerializerType.MirrorBuiltIn)
            // {
            //     CmdHandleRequestMirrorWeaver(request);
            //     return;
            // }
        }
        
        protected override void HandleRequestArgs(object[] args)
        {
            Request = (TReq)args[0];
        }

        // [Server]
        // protected virtual void CmdHandleRequestMirrorWeaver(TReq request, NetworkConnectionToClient sender = null)
        // {
        //     Request = request;
        //     CmdHandleRequest(sender);
        // }

        // [Server]
        // protected virtual void CmdHandleRequestJson(string json, NetworkConnectionToClient sender = null)
        // {
        //     var request = JsonUtility.FromJson<TReq>(json);
        //     Request = request;
        //     CmdHandleRequest(sender);
        // }
    }

    public abstract class Post<TReq, TReq2, TRes> : Post<TReq, TRes>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected TReq2 Request2;

        [Client]
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        public void Send(TReq request, TReq2 request2, ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            Request = request;
            Request2 = request2;
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
            // if (usingSerializerType == RequestSerializerType.JsonUtility)
            // {
            //     var json = JsonUtility.ToJson(request);
            //     var json2 = JsonUtility.ToJson(request2);
            //     CmdHandleRequestJson(json, json2);
            //     return;
            // }
            //
            // if (usingSerializerType == RequestSerializerType.MirrorBuiltIn)
            // {
            //     CmdHandleRequestMirrorWeaver(request, request2);
            //     return;
            // }
        }

        protected override void HandleRequestArgs(object[] args)
        {
            Request = (TReq)args[0];
            Request2 = (TReq2)args[1];
        }

        // [Server]
        // protected virtual void CmdHandleRequestMirrorWeaver(TReq request, TReq2 request2,
        //     NetworkConnectionToClient sender = null)
        // {
        //     Request = request;
        //     Request2 = request2;
        //     CmdHandleRequest(sender);
        // }
        //
        // [Server]
        // protected virtual void CmdHandleRequestJson(string json, string json2, NetworkConnectionToClient sender = null)
        // {
        //     Request = JsonUtility.FromJson<TReq>(json);
        //     Request2 = JsonUtility.FromJson<TReq2>(json2);
        //     CmdHandleRequest(sender);
        // }
    }
}