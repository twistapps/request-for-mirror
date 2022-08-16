﻿using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    public abstract class Post<TRequest, TResponse> : Fetch<TResponse>
    {
        protected TRequest req;

        public void Send(TRequest requestData, ResponseDelegate responseCallback, RequestFailEvent onError = null)
        {
            req = requestData;
            SetupRequest(responseCallback, onError);
            var json = JsonUtility.ToJson(req);
            CmdComputeResponse(json);
        }

        protected void DeserializeRequestData(string requestData)
        {
            req = JsonUtility.FromJson<TRequest>(requestData);
        }

        /// <summary>
        ///     Handle request on Server
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="status"></param>
        /// <param name="json"></param>
        /// <param name="sender"></param>
        protected virtual void HandleRequestOnServer(string requestData, out RequestStatus status, out string json, NetworkConnectionToClient sender = null)
        {
            DeserializeRequestData(requestData);
            base.HandleRequestOnServer(out status, out json, sender);
        }

        protected virtual void CmdComputeResponse(string requestData, NetworkConnectionToClient sender = null)
        {
        }
    }

    public abstract class Post<TRequest, TRequest2, TResponse> : Post<TRequest, TResponse>
    {
        protected TRequest2 req2;
        
        public void Send(TRequest requestData, TRequest2 requestData2, ResponseDelegate responseCallback, RequestFailEvent onError = null)
        {
            req = requestData;
            req2 = requestData2;
            SetupRequest(responseCallback, onError);
            var json = new string[2];
            json[0] = JsonUtility.ToJson(req);
            json[1] = JsonUtility.ToJson(req2);
            CmdComputeResponse(json);
        }

        protected virtual void HandleRequestOnServer(string[] requestData, out RequestStatus status, out string json, NetworkConnectionToClient sender = null)
        {
            DeserializeRequestData(requestData);
            base.HandleRequestOnServer(out status, out json, sender);
        }
        
        protected virtual void DeserializeRequestData(string[] requestData)
        {
            req = JsonUtility.FromJson<TRequest>(requestData[0]);
            req2 = JsonUtility.FromJson<TRequest2>(requestData[1]);
        }
        
        protected virtual void CmdComputeResponse(string[] requestData, NetworkConnectionToClient sender = null)
        {
            //will be autogenerated for each child class
        }
    }
}