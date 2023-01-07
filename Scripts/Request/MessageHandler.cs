using System;
using System.Collections.Generic;
using Mirror;
using RequestForMirror;
using UnityEngine;


public interface INetworkRequest : NetworkMessage
{
    
}

public interface INetworkResponse : NetworkMessage
{
    int ID { get; set; }
    bool HasErrors { get; set; }
}

public interface IMessageHandler
{
    
}

public struct TestRequestData : INetworkRequest
{
    
}

public struct TestResponse : INetworkResponse
{
    public int Num;

    public int ID { get; set; }
    public bool HasErrors { get; set; }
}


// should be autogenerated
public static partial class Request
{
    public static readonly RequestTest requestTest = new RequestTest();

    public static void Init()
    {
        requestTest.Init();
    }
}

public abstract class MessageHandler<TReq, TRes> 
    where TReq : struct, INetworkRequest
    where TRes : struct, INetworkResponse
{
    protected MessageHandler()
    {
        Init();
    }

    public void Init()
    {
        NetworkServer.RegisterHandler<TReq>(HandleRequest);
        NetworkClient.RegisterHandler<TRes>(HandleResponse);
    }

    public delegate void ResponseHandlerDelegate(TRes message);

    private Dictionary<RequestId, ResponseHandlerDelegate> _responseHandlers;

    protected abstract void OnRequest(NetworkConnectionToClient sender, TReq request, out TRes response);

    private void HandleRequest(NetworkConnectionToClient sender, TReq request)
    {
        OnRequest(sender, request, out var response);
        response.ID = (int)RequestIdProvider.GenerateId(sender);
        sender.Send(response);
    }
    
    private void HandleResponse(TRes response)
    {
        var id = new RequestId(response.ID);
        if (!_responseHandlers.ContainsKey(id))
        {
            Debug.LogError($"{GetType().Name}: callback with id {id.ID} not found. Callbacks won't trigger");
            return;
        }
        _responseHandlers[id](response);
        _responseHandlers.Remove(id);
    }

    public void Send(TReq message, ResponseHandlerDelegate onResponse)
    {
        var id = RequestIdProvider.localId.Next();
        _responseHandlers.Add(id, onResponse);
        NetworkClient.Send(message);
    }
}

public class RequestTest : MessageHandler<TestRequestData, TestResponse>
{
    protected override void OnRequest(NetworkConnectionToClient sender, TestRequestData requestData, out TestResponse response)
    {
        response = new TestResponse
        {
            Num = 12,
            HasErrors = true
        };
    }
}