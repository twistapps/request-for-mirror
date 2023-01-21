using Mirror;

public interface INetworkResponse : NetworkMessage
{
    int ID { get; set; }
    bool HasErrors { get; set; }
}