using System;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoStreaming;
using OfflinePacketSimulator;
using Xunit;

namespace MediaLibTests;

[Collection("Sequential")]
public class FrameBufferTests
{
    private OfflinePacketBuffer offlinePacketBuffer = new OfflinePacketBuffer();
    private FrameBuffer  frameBuffer => offlinePacketBuffer.FrameBuffer;

    [Fact]
    public void BufferFullFrameTest()
    {
        var frame = offlinePacketBuffer.GetRandomFullFrame();
        var currentFrameBufferSize = frameBuffer.frameBufferCount;
        frameBuffer.BufferFullFrame(frame.Item1, frame.Item2);
        Assert.Equal(1, frameBuffer.frameBufferCount);
    }
    
    [Fact]
    public void BufferPartialFrameTest()
    {
        var frames = offlinePacketBuffer.GetRandomBlockOfPartialFrames();
        foreach (var frame in frames)
        {
            frameBuffer.BufferPartialFrame(frame.Item1, frame.Item2);
        }
        Assert.Equal(1, frameBuffer.partialFrameBufferCount);
        Assert.Equal(1, frameBuffer.frameBufferCount);
    }
    
    [Fact]
    public void BufferFrameTest()
    {
        var framePacket = offlinePacketBuffer.GetRandomPacket();
        frameBuffer.BufferFrame(framePacket);
        Assert.Equal(1, frameBuffer.PacketCount);
        //We dont know whether the packet will be a partial or a full, we just want to check the packet went somewhere
        var bufferCount = Math.Max(frameBuffer.frameBufferCount, frameBuffer.partialFrameBufferCount);
        Assert.Equal(1, bufferCount);
    }

    [Fact]
    public void GetNextFrameTest()
    {
        //When empty should be false
        Assert.False(frameBuffer.GetNextFrame(out var NextFrame));
        offlinePacketBuffer.SeedFirstFrame();
        Assert.True(frameBuffer.GetNextFrame(out NextFrame));
        
    }


    [Fact]
    public void NewFrameReadyCallbackTest()
    {
        Assert.Equal(0, frameBuffer.frameBufferCount);
        offlinePacketBuffer.SeedFirstFrame();
        Assert.Equal(1, frameBuffer.frameBufferCount);
    }
}
