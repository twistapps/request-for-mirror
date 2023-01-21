public struct TestResponse : INetworkResponse
{
    public int Num;

    public int ID { get; set; }
    public bool HasErrors { get; set; }
}