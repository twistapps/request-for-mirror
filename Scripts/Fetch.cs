﻿using System.Diagnostics.CodeAnalysis;
using Mirror;

namespace RequestForMirror
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class Fetch<TRes> : RequestBase<TRes>
    {
        [Client]
        public void Send(ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
        }

        protected override void HandleRequestArgs(object[] args)
        {
        }
    }
}