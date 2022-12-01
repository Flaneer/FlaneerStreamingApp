using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Class that performs full logging of the UDP connection
/// </summary>
internal class UDPClientStatTracker
{
    private readonly Logger logger;
    private int packetCount;
    private int emptyPacketCount;
    private readonly SimpleMovingAverage bitrateAverage = new SimpleMovingAverage(5);
    private readonly SimpleMovingAverage latencyAverage = new SimpleMovingAverage(5);
    private int lastSecond = DateTime.Now.Second;
    private int bytesThisSecond;
    private uint lastPacket;
    private uint droppedPackets;
    
    /// <summary>
    /// ctor
    /// </summary>
    public UDPClientStatTracker(UDPReceiver receiver)
    {
        logger = Logger.GetLogger(this);
        
        receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, VideoStreamStats);
    }

    private void VideoStreamStats(SmartBuffer packet)
    {
        if (lastSecond != DateTime.Now.Second)
        {
            var averageBitRate = bitrateAverage.Update(bytesThisSecond);
            StatLogging.LogPerfStat("Average Bitrate (KBs)", averageBitRate/1000);
            bytesThisSecond = 0;
            lastSecond = DateTime.Now.Second;
        }
        bytesThisSecond += packet.Length;
        
        long ticks = PacketInfoParser.TimeStamp(packet.Buffer);
        var latencyTicks = DateTime.UtcNow.Ticks - ticks;
        var latency = TimeSpan.FromTicks(latencyAverage.Update(latencyTicks));
        StatLogging.LogPerfStat("Latency(ms)", latency.Milliseconds);

        var packetId = PacketInfoParser.PacketId(packet.Buffer);
        droppedPackets += packetId - (lastPacket + 1);
        lastPacket = packetId;
        StatLogging.LogPerfStat("Dropped Packets", droppedPackets);

        if (packet.Length == 0)
        {
            StatLogging.LogPerfStat("EmptyPackets", ++emptyPacketCount);
        }
    }
}
