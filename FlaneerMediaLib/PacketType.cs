namespace FlaneerMediaLib;

/// <summary>
/// Defines the different types of packets
/// </summary>
public enum PacketType : byte
{
    /// <summary>
    /// A packet that contains a chunk or a whole of video frame and metadata
    /// </summary>
    VideoStreamPacket,
    /// <summary>
    /// A packet that acknowledges a packet of the same Id was received
    /// </summary>
    Ack,
    /// <summary>
    /// A packet containing the info to facilitate UDP hole punching
    /// </summary>
    HolePunchInfo,
    /// <summary>
    /// A packet type used in unit testing
    /// </summary>
    TestPacket = Byte.MaxValue, 
}

/// <summary>
/// Defines the type for the node 
/// </summary>
public enum NodeType : byte
{
    /// <summary>
    /// A node that is sending video
    /// </summary>
    StreamingServer,
    /// <summary>
    /// A node that is receiving video
    /// </summary>
    StreamingClient,
}
