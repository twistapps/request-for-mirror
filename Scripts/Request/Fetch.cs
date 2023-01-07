using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mirror;
using UnityEngine;

namespace RequestForMirror
{

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class Fetch<TRes> : RequestBase<TRes>
    {
        [Client]
        public void Send(ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            InitSend(out _, responseCallback, failCallback);
            CmdHandleRequest();
        }
    }
}