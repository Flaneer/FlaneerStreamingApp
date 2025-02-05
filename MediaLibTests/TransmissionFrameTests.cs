﻿using FlaneerMediaLib.VideoDataTypes;
using Xunit;

namespace MediaLibTests;

public class TransmissionFrameTests
{
    [Fact]
    public void TestHeaderSizeConstIsAccurate()
    {
        TransmissionVideoFrame frame = new TransmissionVideoFrame();
        var frameAsBytes = frame.ToUDPPacket();
        Assert.Equal(frameAsBytes.Length, TransmissionVideoFrame.HeaderSize);
    }
}
