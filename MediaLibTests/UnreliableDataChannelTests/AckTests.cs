using System;
using System.Collections.Generic;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using Xunit;

namespace MediaLibTests;

public class AckTests
{
    [Fact]
    public void TestToAck()
    {
        UInt32 testNum = 4;
        var testBuffer = Ack.BufferFromAck(testNum);
        
        Assert.True(testBuffer[29]);
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

    [Fact]
    public void TestAckReceptionAndParsing32()
    {
        
        Dictionary<int, bool> prevAcks = new Dictionary<int, bool>
        {
            {0, true}, {1, true}, {2, true}, {3, true}, {4, true}, {5, true}, {6, true}, {7, true},
            {8, true}, {9, true}, {10, true}, {11, true}, {12, true}, {13, true}, {14, true}, {15, true},
            {16, true}, {17, true}, {18, true}, {19, true}, {20, true}, {21, true}, {22, true}, {23, true},
            {24, true}, {25, true}, {26, true}, {27, true}, {28, true}, {29, true}, {30, true}, {31, true},
        };
        byte[] packet = {(byte) PacketType.Ack, 32, 0, 0, 0, 255, 255, 255, 255};
        
        AckReceiver.OnAckReceivedImpl(packet, prevAcks);
        
        Assert.Equal(32, prevAcks.Count);
        foreach (var kvp in prevAcks)
        {
            Assert.True(kvp.Value);
        }
    }
    
    [Fact]
    public void TestAckReceptionAndParsingExtraBuffer()
    {
        
        Dictionary<int, bool> prevAcks = new Dictionary<int, bool>
        {
            {0, true}, {1, true}, {2, true}, {3, true}, {4, true}, {5, true}, {6, true}, {7, true},
            {8, true}, {9, true}, {10, true}, {11, true}, {12, true}, {13, true}, {14, true}, {15, true},
            {16, true}, {17, true}, {18, true}, {19, true}, {20, true}, {21, true}, {22, true}, {23, true},
            {24, true}, {25, true}, {26, true}, {27, true}, {28, true}, {29, true}, {30, true}, {31, true},
            {32, true}, {33, true}, {34, true}, {35, true}, {36, true}, {37, true}, {38, true}, {39, true},
        };
        byte[] packet = {(byte) PacketType.Ack, 40, 0, 0, 0, 255, 255, 255, 255};
        
        AckReceiver.OnAckReceivedImpl(packet, prevAcks);
        
        Assert.Equal(40, prevAcks.Count);
        foreach (var kvp in prevAcks)
        {
            Assert.True(kvp.Value);
        }
    }
    
    [Fact]
    public void TestAckReceptionAndParsingPartialBuffer()
    {
        
        Dictionary<int, bool> prevAcks = new Dictionary<int, bool>
        {
            {0, true}, {1, true}, {2, true}, {3, true}, {4, true}, {5, true}, {6, true}, {7, true}, {8, true}, {9, true}
        };
        byte[] packet = {(byte) PacketType.Ack, 10, 0, 0, 0, 255, 3, 0, 0};
        
        AckReceiver.OnAckReceivedImpl(packet, prevAcks);
        
        Assert.Equal(10, prevAcks.Count);
        foreach (var kvp in prevAcks)
        {
            Assert.True(kvp.Value);
        }
    }
    
    [Fact]
    public void TestAckReceptionAndParsingPartialBufferWithSomeFalse()
    {
        
        Dictionary<int, bool> prevAcks = new Dictionary<int, bool>
        {
            {0, false}, {1, true}, {2, false}, {3, true}, {4, false}, {5, true}, {6, false}, {7, true}, {8, false}, {9, true}
        };
        var x =BitConverter.GetBytes(682);
        byte[] packet = {(byte) PacketType.Ack, 10, 0, 0, 0, 170, 2, 0, 0};
        
        AckReceiver.OnAckReceivedImpl(packet, prevAcks);
        
        Assert.Equal(10, prevAcks.Count);
        var flipFlop = false;
        foreach (var kvp in prevAcks)
        {
            Assert.Equal(flipFlop, kvp.Value);
            flipFlop = !flipFlop;
        }
    }
}
