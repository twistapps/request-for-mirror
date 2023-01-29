using System.Diagnostics.CodeAnalysis;
#if MIRROR
using Mirror;
#endif

namespace RequestForMirror
{
    #if REQUESTIFY_ENABLED
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class Fetch<TRes> : RequestBase<TRes>
    {
        #if MIRROR
        [Client]
        #endif
        public void Send(ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
        }

        protected override void HandleRequestArgs(object[] args)
        {
        }
    }
    #endif
}