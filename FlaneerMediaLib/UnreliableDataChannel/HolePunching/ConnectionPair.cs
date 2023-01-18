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
    /// The server node
    /// </summary>
    public HolePunchInfoPacket Server => server;
    private HolePunchInfoPacket? server;

    /// <summary>
    /// Both a client and server are present and the connection is made.
    /// </summary>
    public bool PairMade => client != null && server != null;

    /// <summary>
    /// ctor
    /// </summary>
    public ConnectionPair(HolePunchInfoPacket holePunchInfoPacket)
    {
        RegisterClient(holePunchInfoPacket);
    }

    /// <summary>
    /// Register a new client
    /// </summary>
    /// <returns>Returns true if both clients are present</returns>
    public bool RegisterClient(HolePunchInfoPacket holePunchInfoPacket)
    {
        switch (holePunchInfoPacket.NodeType)
        {
            case NodeType.StreamingClient: client = holePunchInfoPacket; break;
            case NodeType.StreamingServer: server = holePunchInfoPacket; break;
        }

        return PairMade;
    }
}
