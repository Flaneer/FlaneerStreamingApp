namespace FlaneerMediaLib.UnreliableDataChannel.HolePunching;

/// <summary>
/// Manages a pair of a client and server on the hole punch server.
/// </summary>
public class ConnectionPair
{
    /// <summary>
    /// The client node
    /// </summary>
    public HolePunchInfoPacket Client => client;
    private HolePunchInfoPacket? client;

    /// <summary>
    /// Whether the client is connected
    /// </summary>
    public bool ClientIsConnected => client != null;
    
    /// <summary>
    /// The server node
    /// </summary>
    public HolePunchInfoPacket Server => server;
    private HolePunchInfoPacket? server;

    /// <summary>
    /// Whether the server is connected
    /// </summary>
    public bool ServerIsConnected => server != null;
    
    /// <summary>
    /// The last time we received a packet from the client
    /// </summary>
    public DateTime LastClientUpdate { get; private set; }
    
    /// <summary>
    /// The last time we received a packet from the server
    /// </summary>
    public DateTime LastServerUpdate { get; private set; }
    
    /// <summary>
    /// Both a client and server are present and the connection is made.
    /// </summary>
    public bool Paired => client != null && server != null;

    /// <summary>
    /// ctor
    /// </summary>
    public ConnectionPair(HolePunchInfoPacket holePunchInfoPacket)
    {
        RegisterClient(holePunchInfoPacket);
    }
    
    /// <summary>
    /// Sets the time for the last heartbeat from the node
    /// </summary>
    public void SetLastUpdate(HolePunchInfoPacket holePunchInfoPacket)
    {
        if (holePunchInfoPacket.HolePunchMessageType == HolePunchMessageType.StreamingClient)
            LastClientUpdate = DateTime.UtcNow;
        else if (holePunchInfoPacket.HolePunchMessageType == HolePunchMessageType.StreamingServer)
            LastServerUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Register a new client
    /// </summary>
    /// <returns>Returns true if both clients are present</returns>
    public bool RegisterClient(HolePunchInfoPacket holePunchInfoPacket)
    {
        switch (holePunchInfoPacket.HolePunchMessageType)
        {
            case HolePunchMessageType.StreamingClient: client = holePunchInfoPacket; break;
            case HolePunchMessageType.StreamingServer: server = holePunchInfoPacket; break;
        }

        return Paired;
    }
    
    /// <summary>
    /// Removes the given node type from the pair
    /// </summary>
    /// <returns>Returns true if you have changed the state, or false if you have made no change</returns>
    public bool RemoveClient(HolePunchMessageType holePunchMessageType)
    {
        var ret = false;
        switch (holePunchMessageType)
        {
            case HolePunchMessageType.StreamingClient:
            {
                ret = client != null;
                client = null; 
                break;
            }
            case HolePunchMessageType.StreamingServer:
            {
                ret = server != null;
                server = null; 
                break;
            }
        }

        return ret;
    }
}
