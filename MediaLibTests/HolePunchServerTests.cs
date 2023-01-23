using System;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;
using Xunit;

namespace MediaLibTests;

public class HolePunchServerTests
{
    [Fact]
    public void TestHolePunchPairingStandard()
    {
        HolePunchServer holePunchServer = new HolePunchServer(true);
        
        HolePunchInfoPacket holePunchInfoPacket1 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket holePunchInfoPacket2 = new HolePunchInfoPacket(HolePunchMessageType.StreamingServer, UInt16.MinValue);
        
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket1));
        Assert.True(holePunchServer.AttemptPairing(holePunchInfoPacket2));
    }
    
    [Fact]
    public void TestHolePunchPairingDoubleClient()
    {
        HolePunchServer holePunchServer = new HolePunchServer(true);
        
        HolePunchInfoPacket holePunchInfoPacket1 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket holePunchInfoPacket2 = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket1));
        Assert.False(holePunchServer.AttemptPairing(holePunchInfoPacket2));
    }
}
