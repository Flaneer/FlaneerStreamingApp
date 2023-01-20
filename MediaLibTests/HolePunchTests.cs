using System;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;
using Xunit;

namespace MediaLibTests;

public class HolePunchTests
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

    [Fact]
    public void TestHolePunchPairingStandard()
    {
        HolePunchServer holePunchServer = new HolePunchServer();
        
        HolePunchInfoPacket holePunchInfoPacket1 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket holePunchInfoPacket2 = new HolePunchInfoPacket(HolePunchMessageType.StreamingServer, UInt16.MinValue);
        
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket1));
        //This would return true if connection was successful, however it will always throw as the server is not running
        Assert.Throws<SocketException>(() => holePunchServer.AttemptPairing(holePunchInfoPacket2));
    }
    
    [Fact]
    public void TestHolePunchPairingDoubleClient()
    {
        HolePunchServer holePunchServer = new HolePunchServer();
        
        HolePunchInfoPacket holePunchInfoPacket1 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket holePunchInfoPacket2 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket1));
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket2));
    }
}
