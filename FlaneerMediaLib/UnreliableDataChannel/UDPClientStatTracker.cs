using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Class that performs full logging of the UDP connection
/// </summary>
public class UDPClientStatTracker
{
    private readonly Logger logger;
    private int packetCount;
    private int emptyPacketCount;
    private SimpleMovingAverage bitrateAverage = new SimpleMovingAverage(5);
    private SimpleMovingAverage latencyAverage = new SimpleMovingAverage(5);
    private int lastSecond = DateTime.Now.Second;
    private int bytesThisSecond = 0;
    private uint lastPacket = 0;
    private uint droppedPackets = 0;
    
    /// <summary>
    /// ctor
    /// </summary>
    public UDPClientStatTracker(UDPReceiver receiver)
    {
        logger = Logger.GetLogger(this);
        
        receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, VideoStreamStats);
    }

    private void VideoStreamStats(byte[] packet)
    {
        if (lastSecond != DateTime.Now.Second)
        {
            var averageBitRate = bitrateAverage.Update(bytesThisSecond);
            StatLogging.LogPerfStat("Average Bitrate (KBs)", averageBitRate/1000);
            bytesThisSecond = 0;
            lastSecond = DateTime.Now.Second;
        }
        bytesThisSecond += packet.Length;
        
        long ticks = PacketInfoParser.TimeStamp(packet);
        var latencyTicks = DateTime.UtcNow.Ticks - ticks;
        var latency = TimeSpan.FromTicks(latencyAverage.Update(latencyTicks));
        StatLogging.LogPerfStat("Latency(ms)", latency.Milliseconds);

        var packetId = PacketInfoParser.PacketId(packet);
        droppedPackets += packetId - (lastPacket + 1);
        lastPacket = packetId;
        StatLogging.LogPerfStat("Dropped Packets", droppedPackets);

        if (packet.Length == 0)
        {
            StatLogging.LogPerfStat("EmptyPackets", ++emptyPacketCount);
        }
    }
}
