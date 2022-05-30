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
}