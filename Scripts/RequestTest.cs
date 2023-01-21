using Mirror;

public class RequestTest : MessageHandler<TestRequestData, TestResponse>
{
    protected override void OnRequest(NetworkConnectionToClient sender, TestRequestData requestData,
        out TestResponse response)
    {
        response = new TestResponse
        {
            Num = 12,
            HasErrors = true
        };
    }
}