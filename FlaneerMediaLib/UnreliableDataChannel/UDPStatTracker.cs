using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib;

/// <summary>
/// Class that performs full logging of the UDP connection
/// </summary>
public class UDPStatTracker
{
    private readonly Logger logger;
    private int emptyPacketCount;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPStatTracker(UDPReceiver receiver)
    {
        logger = Logger.GetLogger(this);
        receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, VideoStreamStats);
    }

    private void VideoStreamStats(byte[] packet)
    {
        logger.Trace($"PacketSize: {packet.Length}");
        if (packet.Length == 0)
        {
            StatLogging.LogPerfStat("EmptyPackets", ++emptyPacketCount);
        }
    }
}
