#if MIRROR
using Mirror;
#endif

namespace RequestForMirror
{
    #if REQUESTIFY_ENABLED
    public abstract class Post<TReq, TRes> : RequestBase<TRes>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected TReq Request;

        #if MIRROR
        [Client]
        #endif
        public void Send(TReq request, ResponseDelegate responseCallback, FailDelegate failCallback = null)
        {
            Request = request;
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
        }

        protected override void HandleRequestArgs(object[] args)
        {
            Request = (TReq)args[0];
        }
    }

    public abstract class Post<TReq, TReq2, TRes> : Post<TReq, TRes>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected TReq2 Request2;

        #if MIRROR
        [Client]
        #endif
        public void Send(TReq request, TReq2 request2, ResponseDelegate responseCallback,
            FailDelegate failCallback = null)
        {
            Request = request;
            Request2 = request2;
            RegisterResponseCallbacks(responseCallback, failCallback);
            Receiver.SendRequest(this);
        }

        protected override void HandleRequestArgs(object[] args)
        {
            Request = (TReq)args[0];
            Request2 = (TReq2)args[1];
        }
    }

    // public abstract class Post<TReq, TRes, TBroadcast> : RequestBase<TRes, TBroadcast>
    // {
    //     protected TReq Request;
    //     
    //     public void Send(TReq request, ResponseDelegate responseCallback, FailDelegate failCallback = null)
    //     {
    //         Request = request;
    //         RegisterResponseCallbacks(responseCallback, failCallback);
    //         Receiver.SendRequest(this);
    //     }
    //
    //     protected override void HandleRequestArgs(object[] args)
    //     {
    //         Request = (TReq)args[0];
    //     }
    // }
    #endif
}