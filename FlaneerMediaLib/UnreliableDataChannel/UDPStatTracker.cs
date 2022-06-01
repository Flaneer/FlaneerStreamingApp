using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib;

internal class SimpleMovingAverage
{
    private readonly int _k;
    private readonly int[] _values;

    private int _index = 0;
    private int _sum = 0;

    public SimpleMovingAverage(int k)
    {
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

        _k = k;
        _values = new int[k];
    }

    public int Update(int nextInput)
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
    private SimpleMovingAverage latency = new SimpleMovingAverage(5);
    private int lastSecond = DateTime.Now.Second;
    private int bytesThisSecond = 0;
    
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
            StatLogging.LogPerfStat("Average Bitrate", averageBitRate/1000);
            bytesThisSecond = 0;
            lastSecond = DateTime.Now.Second;
        }
        bytesThisSecond += packet.Length;
        
        long ticks = BitConverter.ToInt64(packet, 3);
        var latencyTicks = DateTime.UtcNow.Ticks - ticks;
        var latency = TimeSpan.FromTicks(latencyTicks);
        StatLogging.LogPerfStat("Latency", latency);
        
        StatLogging.LogPerfStat("Packets Received", ++packetCount);
        if (packet.Length == 0)
        {
            StatLogging.LogPerfStat("EmptyPackets", ++emptyPacketCount);
        }
    }
}
