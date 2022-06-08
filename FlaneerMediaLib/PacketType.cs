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
}
