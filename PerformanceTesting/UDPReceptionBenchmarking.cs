using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;
using OfflinePacketSimulator;

namespace PerformanceTesting;

[MemoryDiagnoser][CsvExporter]
public class UDPReceptionBenchmarking
{
    private OfflinePacketBuffer offlinePacketBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        offlinePacketBuffer = new OfflinePacketBuffer();
    }

    [Benchmark]
    public void ProcessReceptionPacket()
    {
        offlinePacketBuffer.UDPReceiver.ProcessReceivedPacket(offlinePacketBuffer.GetRandomPacket());
    }
    
    [Benchmark]
    public TransmissionVideoFrame TransmissionFrameParse()
    {
        return TransmissionVideoFrame.FromUDPPacket(offlinePacketBuffer.GetRandomPacket().Buffer);
    }

    [Benchmark]
    public void FullFrameBufferH264()
    {
        var randomFullFrame = offlinePacketBuffer.GetRandomFullFrame();
        offlinePacketBuffer.FrameBuffer.BufferFullFrame(randomFullFrame.Item1, randomFullFrame.Item2);
    }
    
    [Benchmark]
    public void PartialFrameBufferH264()
    {
        foreach (var partialFrame in offlinePacketBuffer.GetRandomBlockOfPartialFrames())
        {
            offlinePacketBuffer.FrameBuffer.BufferPartialFrame(partialFrame.Item1, partialFrame.Item2);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        offlinePacketBuffer.RefreshFrameBuffer();
    }

    [Benchmark]
    public void GetFrames()
    {
        offlinePacketBuffer.FrameBuffer.GetNextFrame(out var nextFrame);
    }
}
