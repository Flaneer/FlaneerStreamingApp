using System;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;
using Xunit;

namespace MediaLibTests;

public class HolePunchInfoPacketTests
{
    [Fact]
    public void TestHeaderSize()
    {
        HolePunchInfoPacket holePunchInfoPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        var packet = holePunchInfoPacket.ToUDPPacket();
        Assert.Equal(HolePunchInfoPacket.HeaderSize, packet.Length);
    }

    [Fact]
    public void TestToFromUDPPacket()
    {
        HolePunchInfoPacket holePunchInfoPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        var packetIn = holePunchInfoPacket.ToUDPPacket();
        var packetOut = HolePunchInfoPacket.FromBytes(packetIn);
        
        Assert.Equal(holePunchInfoPacket.PacketType, packetOut.PacketType);
        Assert.Equal(holePunchInfoPacket.PacketSize, packetOut.PacketSize);
        Assert.Equal(holePunchInfoPacket.PacketId, packetOut.PacketId);
        Assert.Equal(holePunchInfoPacket.ConnectionId, packetOut.ConnectionId);
        Assert.Equal(holePunchInfoPacket.HolePunchMessageType, packetOut.HolePunchMessageType);
        //Check the timestamp is within 1 second of the current time, it is encoded when ToUDPPacket is called so is not in the original packet
        Assert.True(DateTime.UtcNow.Ticks - packetOut.TimeStamp < TimeSpan.TicksPerSecond);
    }

    [Fact]
    public void TestToFromIpEndpoint()
    {
        HolePunchInfoPacket holePunchInfoPacket = new HolePunchInfoPacket("8.8.8.8", 42069);
        var ipEndPoint = holePunchInfoPacket.ToEndPoint();
        var packetOut = HolePunchInfoPacket.FromEndPoint(ipEndPoint, HolePunchMessageType.StreamingServer, UInt16.MinValue);
        
        Assert.Equal(holePunchInfoPacket.ToString(), packetOut.ToString());
    }
}
