using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using OfflinePacketSimulator;

namespace PerformanceTesting;

public class ArrayCopyBenchmarking
{
    private List<byte[]> rawPackets = new List<byte[]>();
    private List<byte[]> destinationPackets = new List<byte[]>();

    private const int NumberOfPackets = 200;

    private int packetLength = 32767;

    public ArrayCopyBenchmarking()
    {
        for (int i = 0; i < NumberOfPackets; i++)
        {
            var packet = File.ReadAllBytes(OfflinePacketAccess.GetPacket(i));
            rawPackets.Add(packet);
            destinationPackets.Add(new byte[packet.Length]);
        }
    }
    
    [Benchmark]
    public void BlockCopy()
    {
        Random rnd = new Random();
        int num = rnd.Next(NumberOfPackets);
        var src = rawPackets[num];
        var dst = destinationPackets[num];
        Buffer.BlockCopy(src, 0, dst, 0, packetLength);
    }

    [Benchmark]
    public void ArrayCopy()
    {
        Random rnd = new Random();
        int num = rnd.Next(NumberOfPackets);
        var src = rawPackets[num];
        var dst = destinationPackets[num];
        Array.Copy(src, dst, packetLength);
    }

    [Benchmark]
    public void ParallelFor()
    {
        Random rnd = new Random();
        int num = rnd.Next(NumberOfPackets);
        var src = rawPackets[num];
        var dst = destinationPackets[num];
        Parallel.For(0, packetLength, idx =>
        {
            dst[idx] = src[idx];
        });
    }

    [Benchmark]
    public unsafe void PointerCopyLoop()
    {
        Random rnd = new Random();
        int num = rnd.Next(NumberOfPackets);
        var src = rawPackets[num];
        var dst = destinationPackets[num];

        fixed (byte* pSrc = src, pDst = dst)
        {
            for (int i = 0; i < packetLength; i++)
            {
                pDst[i] = pSrc[i];
            }
        }
    }
    
    [Benchmark]
    public unsafe void MemoryCopy()
    {
        Random rnd = new Random();
        int num = rnd.Next(NumberOfPackets);
        var src = rawPackets[num];
        var dst = destinationPackets[num];

        fixed (byte* pSrc = src, pDst = dst)
        {
            Buffer.MemoryCopy(pSrc, pDst, packetLength, packetLength);
        }
    }
}
