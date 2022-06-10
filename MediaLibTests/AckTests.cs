using System;
using System.Collections.Generic;
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
        UInt32 testNum = Ack.AcksFromBinary(testBuffer);
        
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

    [Fact]
    public void TestSendAck()
    {
        var ackBuffer = new List<UInt32>(){};

        for (UInt32 i = 0; i < 64; i++)
        {
            var testPacket = new TestPacket()
            {
                PacketId = i
            };
            var testPacketBytes = testPacket.ToUDPPacket();
            
            var testAck = AckSender.AckFromReceivedPacket(ackBuffer, testPacketBytes);
            
            Assert.Equal(i, testAck.PacketId);
            if (i <= 32)
            {
                var ackBin = "";
                ackBin = ackBin.PadLeft((int)i, '1');
                ackBin = ackBin.PadLeft(32, '0');
                var expectedFromBinary = Convert.ToUInt32(ackBin, 2);
                Assert.Equal(expectedFromBinary, testAck.PreviousAcks);
            }
            else
            {
                Assert.Equal(UInt32.MaxValue, testAck.PreviousAcks);
            }
        }
    }
    
    [Fact]
    public void TestSendAckWithPacketLoss()
    {
        var ackBuffer = new List<UInt32>(){};

        for (UInt32 i = 0; i < 66; i+=2)
        {
            var testPacket = new TestPacket()
            {
                PacketId = i
            };
            var testPacketBytes = testPacket.ToUDPPacket();
            
            var testAck = AckSender.AckFromReceivedPacket(ackBuffer, testPacketBytes);
            
            Assert.Equal(i, testAck.PacketId);
            if (i == 65)
            {
                var expected = Convert.ToUInt32("1010101010101010101010101010101", 2);
                Assert.Equal(expected, testAck.PreviousAcks);
            }
        }
    }
}
