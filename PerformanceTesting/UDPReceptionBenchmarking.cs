using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace PerformanceTesting;

[MemoryDiagnoser][CsvExporter]
public class UDPReceptionBenchmarking
{
    private FrameBuffer fbH264 = new FrameBuffer(VideoCodec.H264);

    private PacketScanner packetScanner = new PacketScanner();

    private List<byte[]> rawPackets = new List<byte[]>();

    private List<Tuple<TransmissionVideoFrame, byte[]>> fullFrameData = new();
    
    private List<List<Tuple<TransmissionVideoFrame, byte[]>>> partialFrameData = new();

    private const int NumberOfPackets = 200;

    private UDPReceiver udpReceiver = new UDPReceiver();

    public UDPReceptionBenchmarking()
    {
        for (int i = 0; i < NumberOfPackets; i++)
        {
            rawPackets.Add(File.ReadAllBytes(BenchmarkingUtils.GetPacket(i)));
        }

        packetScanner.PopulateFullFrameList(fullFrameData);
        packetScanner.PopulatePartialFrameList(partialFrameData);
    }

    [GlobalSetup]
    public void SeedFrameBuffer()
    {
        foreach (var frameData in fullFrameData)
        {
            fbH264.BufferFullFrame(frameData.Item1, frameData.Item2);
        }
        udpReceiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, delegate { });
    }

    [Benchmark]
    public void ProcessReceptionPacket()
    {
        Random rnd = new Random();
        int num = rnd.Next(rawPackets.Count);
        udpReceiver.ProcessReceivedPacket(rawPackets[num]);
    }
    
    [Benchmark]
    public TransmissionVideoFrame TransmissionFrameParse()
    {
        Random rnd = new Random();
        int num = rnd.Next(rawPackets.Count);
        return TransmissionVideoFrame.FromUDPPacket(rawPackets[num]);
    }

    [Benchmark]
    public void FullFrameBufferH264()
    {
        Random rnd = new Random();
        int num = rnd.Next(fullFrameData.Count);
        fbH264.BufferFullFrame(fullFrameData[num].Item1, fullFrameData[num].Item2);
    }
    
    [Benchmark]
    public void PartialFrameBufferH264()
    {
        Random rnd = new Random();
        int num = rnd.Next(partialFrameData.Count);
        foreach (var partialFrame in partialFrameData[num])
        {
            fbH264.BufferPartialFrame(partialFrame.Item1, partialFrame.Item2);
        }
    }

    [Benchmark]
    public void GetFrames()
    {
        fbH264.GetNextFrame(out var nextFrame);
    }
}
