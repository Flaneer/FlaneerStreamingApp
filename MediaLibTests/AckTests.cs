using System;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class AckTests
{
    [Fact]
    public void TestToAck()
    {
        UInt32 testNum = 4;
        var testBuffer = Ack.BufferFromAck(testNum);
        
        Assert.Equal(1, testBuffer[29]);
    }

    [Fact]
    public void TestFromAck()
    {
        int[] testBuffer =
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,};
        UInt32 testNum = Ack.AcksFromBuffer(testBuffer);
        
        Assert.Equal((UInt32)4, testNum);
    }
}
