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

    [Fact]
    public void TestToUDP()
    {
        Ack testAck = new Ack();
        testAck.PacketId = 10;
        testAck.PreviousAcks = 4;

        var testBuffer = testAck.ToUDPPacket();
        
        Assert.Equal(Ack.ACKSIZE, testBuffer.Length);

        byte[] expectedBuffer = {(byte) PacketType.Ack, 10, 0, 0, 0, 4, 0, 0, 0};
        
        Assert.Equal(expectedBuffer, testBuffer);
    }

    [Fact]
    public void TestFromUDP()
    {
        byte[] packet = {(byte) PacketType.Ack, 10, 0, 0, 0, 4, 0, 0, 0};
        Ack testAck = Ack.FromUDPPacket(packet);
        
        Assert.Equal((UInt32)10, testAck.PacketId);
        Assert.Equal((UInt32)4, testAck.PreviousAcks);
    }
}
