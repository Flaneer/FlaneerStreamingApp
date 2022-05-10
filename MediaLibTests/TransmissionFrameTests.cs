using System;
using System.Reflection;
using FlaneerMediaLib.VideoDataTypes;
using Xunit;

namespace MediaLibTests;

public class TransmissionFrameTests
{
    [Fact]
    public void DummyTest()
    {
        TransmissionVideoFrame frame = new TransmissionVideoFrame();
        var frameAsBytes = frame.ToUDPPacket();
        Assert.Equal(frameAsBytes.Length, TransmissionVideoFrame.HeaderSize);
    }
}
