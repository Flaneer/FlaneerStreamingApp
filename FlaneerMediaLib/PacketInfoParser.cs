namespace FlaneerMediaLib;

/// <summary>
/// Contains the indices where each <see cref="IPacketInfo"/> property starts in the transmission array 
/// </summary>
public static class PacketInfoParser
{
    /// <summary>
    /// <see cref="IPacketInfo.PacketType"/>
    /// </summary>
    public const int PacketTypeIdx = 0;
    /// <summary>
    /// <see cref="IPacketInfo.PacketSize"/>
    /// </summary>
    public const int PacketSizeIdx = 1;
    /// <summary>
    /// <see cref="IPacketInfo.TimeStamp"/>
    /// </summary>
    public const int TimeStampIdx = 3;
    /// <summary>
    /// <see cref="IPacketInfo.PacketId"/>
    /// </summary>
    public const int PacketIdIdx = 11;
    
    /// <summary>
    /// <see cref="IPacketInfo.PacketType"/>
    /// </summary>
    public static PacketType PacketType(byte[] packet) => (PacketType) packet[PacketTypeIdx];
    /// <summary>
    /// <see cref="IPacketInfo.PacketSize"/>
    /// </summary>
    public static ushort PacketSize(byte[] packet) => BitConverter.ToUInt16(packet, PacketSizeIdx);
    /// <summary>
    /// <see cref="IPacketInfo.TimeStamp"/>
    /// </summary>
    public static long TimeStamp(byte[] packet) => BitConverter.ToInt64(packet, TimeStampIdx);
    /// <summary>
    /// <see cref="IPacketInfo.PacketId"/>
    /// </summary>
    public static UInt32 PacketId(byte[] packet) => BitConverter.ToUInt32(packet, PacketIdIdx);
}
