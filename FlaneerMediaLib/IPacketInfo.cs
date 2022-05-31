namespace FlaneerMediaLib;

/// <summary>
/// Base class for all packets
/// </summary>
public interface IPacketInfo
{
    /// <summary>
    /// The type of this packet
    /// </summary>
    PacketType PacketType { get; }
    /// <summary>
    /// The size of the packet at transmission, this can be used in cases where multiple packets are in the buffer
    /// </summary>
    ushort PacketSize { get; set; }
}
