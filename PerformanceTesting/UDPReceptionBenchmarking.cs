using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace PerformanceTesting;

[MemoryDiagnoser]
public class UDPReceptionBenchmarking
{
    private FrameBuffer fbH264 = new FrameBuffer(VideoCodec.H264);

    private PacketScanner packetScanner = new PacketScanner();

    private List<byte[]> rawPackets = new List<byte[]>();

    private List<Tuple<TransmissionVideoFrame, byte[]>> fullFrameData = new();
    
    private List<List<Tuple<TransmissionVideoFrame, byte[]>>> partialFrameData = new();

    private const int NumberOfPackets = 100;

    public UDPReceptionBenchmarking()
    {
        for (int i = 0; i < NumberOfPackets; i++)
        {
            rawPackets.Add(File.ReadAllBytes($"C:/Users/Tom/Code/FlaneerStreamingApp/TestResources/SamplePackets/packet{i}.bin"));
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
