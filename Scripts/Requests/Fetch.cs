﻿using Mirror;
using UnityEngine;

namespace RequestForMirror
{
    /// <summary>
    ///     Send a request to a Mirror server and get a callback when the response is received.
    /// </summary>
    /// <typeparam name="TResponse">Type of data on response</typeparam>
    public abstract class Fetch<TResponse> : NetworkBehaviour, IMarkedForCodeGen
    {
        public delegate void RequestFailEvent(string reason);

        public delegate void ResponseDelegate(TResponse res);

        protected RequestFailEvent onError;
        protected ResponseDelegate onResponse;

        protected RequestResponse<TResponse> res;

        //server only
        protected NetworkConnectionToClient sender;

        protected void SetupRequest(ResponseDelegate responseCallback, RequestFailEvent onError = null)
        {
            res = new RequestResponse<TResponse>();
            onResponse = responseCallback;
            this.onError = onError;
        }

        public virtual void Send(ResponseDelegate responseCallback, RequestFailEvent onError = null)
        {
            SetupRequest(responseCallback, onError);
            CmdComputeResponse();
        }

        /// <summary>
        ///     What to do on SERVER before sending response.
        /// </summary>
        /// <param name="status">If something went wrong, specify it here.</param>
        protected abstract void OnRequest(out RequestStatus status);

        /// <summary>
        ///     Autogenerated on compile
        /// </summary>
        protected virtual void CmdComputeResponse(NetworkConnectionToClient sender = null)
        {
        }

        /// <summary>
        ///     Handle request on Server
        /// </summary>
        /// <param name="status"></param>
        /// <param name="json">TResponse in json format</param>
        /// <param name="sender">Autofilled by mirror</param>
        protected virtual void HandleRequestOnServer(out RequestStatus status, out string json,
            NetworkConnectionToClient sender = null)
        {
            res = new RequestResponse<TResponse>();
            this.sender = sender;
            OnRequest(out status);

            //todo: autogenerated types for mirror serializer instead of manual serialization to json
            json = JsonUtility.ToJson(res.data);
        }

        /// <summary>
        ///     Handle received response. Scope: Client
        /// </summary>
        /// <param name="status"></param>
        /// <param name="response"></param>
        protected virtual void HandleResponseOnClient(RequestStatus status, string response)
        {
            res.SetResponse(JsonUtility.FromJson<TResponse>(response));
            if (status.hasErrors)
                onError?.Invoke(status.errorMessage);
            else
                onResponse?.Invoke(res.data);
        }
    }

    public class RequestResponse<TResponse>
    {
        public TResponse data;

        public void SetResponse(TResponse response)
        {
            data = response;
        }
    }

    public class RequestStatus
    {
        public readonly string errorMessage;
        public readonly bool hasErrors;

        public RequestStatus(bool ok, string errorMessage = null)
        {
            hasErrors = !ok;
            this.errorMessage = errorMessage;
        }

        //required by Mirror's serializer
        public RequestStatus()
        {
        }
    }
}