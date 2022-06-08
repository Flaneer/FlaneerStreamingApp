using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib;

internal class SimpleMovingAverage
{
    private readonly long _k;
    private readonly long[] _values;

    private long _index = 0;
    private long _sum = 0;

    public SimpleMovingAverage(long k)
    {
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

        _k = k;
        _values = new long[k];
    }

    public long Update(long nextInput)
    {
        // calculate the new sum
        _sum = _sum - _values[_index] + nextInput;

        // overwrite the old value with the new one
        _values[_index] = nextInput;

        // increment the index (wrapping back to 0)
        _index = (_index + 1) % _k;

        // calculate the average
        return  _sum / _k;
    }
}


/// <summary>
/// Class that performs full logging of the UDP connection
/// </summary>
public class UDPStatTracker
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
    public UDPStatTracker(UDPReceiver receiver)
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
        StatLogging.LogPerfStat("Latency", latency);

        var packetId = PacketInfoParser.PacketId(packet);
        droppedPackets += packetId - (lastPacket + 1);
        lastPacket = packetId;
        StatLogging.LogPerfStat("Dropped Packets", droppedPackets);
        
        
        StatLogging.LogPerfStat("Packets Received", ++packetCount);
        if (packet.Length == 0)
        {
            StatLogging.LogPerfStat("EmptyPackets", ++emptyPacketCount);
        }
    }
}
