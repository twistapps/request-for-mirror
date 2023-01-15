// ReSharper disable once RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using RequestForMirror;
using TwistCore;

public class TargetReceiver : NetworkBehaviour, IMarkedForCodeGen
{
    public static NetworkIdentity GlobalRequestManager = null;

    private static readonly Dictionary<int, TargetReceiver> ReceiversByConnId = new Dictionary<int, TargetReceiver>();
    private static TargetReceiver _localReceiver;

    private static TargetReceiver GetCachedReceiver(int connId, bool isServer = true)
    {
        TargetReceiver receiver = null;

        if (!isServer && _localReceiver != null)
            return _localReceiver;

        if (isServer && ReceiversByConnId.ContainsKey(connId))
        {
            receiver = ReceiversByConnId[connId];
        }
        else
        {
            var ply = isServer ? NetworkServer.connections[connId] : NetworkClient.connection;
            foreach (var networkIdentity in ply.owned)
            {
                receiver = networkIdentity.GetComponent<TargetReceiver>();
                if (receiver == null) continue;

                if (isServer) ReceiversByConnId[connId] = receiver;
                else _localReceiver = receiver;

                break;
            }
        }

        return receiver;
    }

    private IRequest FindAwaitingResponse(IEnumerable<IRequest> requests, int requestId)
    {
        return requests?.FirstOrDefault(r => r.IsAwaitingResponse(requestId));
    }

    public IRequest FindAwaitingResponse(int requestId)
    {
        return FindAwaitingResponse(GetComponents<IRequest>(), requestId) ??
               // ReSharper disable once Unity.NoNullPropagation
               FindAwaitingResponse(GlobalRequestManager?.GetComponents<IRequest>(), requestId);
    }

    public static void SendResponse<TRes>(NetworkConnection target,
        int requestID,
        Status status,
        TRes response)
    {
        var receiver = GetCachedReceiver(target.connectionId);

        object[] parameters =
        {
            receiver, target, requestID, status, response
        };

        typeof(TargetReceiver).GetMethod("TargetReceiveResponseMirrorWeaver")?.Invoke(receiver, parameters);
        //receiver.TargetReceiveResponseMirrorWeaver(target, id, status, response);
    }

    private void PushResponseOnClient<TRes>(NetworkConnection target,
        int requestID,
        Status status,
        TRes response)
    {
        var request = (RequestBase<TRes>)FindAwaitingResponse(requestID);
        request.TargetReceiveResponseMirrorWeaver(target, requestID, status, response);
    }


    //////////////
    //$CLASS_INNER$ - Code Generation variable
    //////////////
}