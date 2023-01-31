using System;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;
using Xunit;

namespace MediaLibTests;

public class HolePunchConnectionPairTests
{
    [Fact]
    public void TestRegistration()
    {
        HolePunchInfoPacket clientPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket serverPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingServer, UInt16.MaxValue);
        
        ConnectionPair pair = new ConnectionPair(clientPacket);
        Assert.True(pair.ClientIsConnected);
        Assert.False(pair.ServerIsConnected);
        Assert.False(pair.Paired);

        pair.RegisterClient(serverPacket);
        Assert.True(pair.ClientIsConnected);
        Assert.True(pair.ServerIsConnected);
        Assert.True(pair.Paired);
    }
    
    [Fact]
    public void TestRemoval()
    {
        HolePunchInfoPacket clientPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket serverPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingServer, UInt16.MaxValue);
        
        ConnectionPair pair = new ConnectionPair(clientPacket);
        pair.RegisterClient(serverPacket);
        Assert.True(pair.ClientIsConnected);
        Assert.True(pair.ServerIsConnected);
        Assert.True(pair.Paired);

        Assert.True(pair.RemoveClient(HolePunchMessageType.StreamingClient));
        Assert.False(pair.RemoveClient(HolePunchMessageType.StreamingClient));
        Assert.False(pair.ClientIsConnected);
        Assert.True(pair.ServerIsConnected);
        Assert.False(pair.Paired);
    }

    [Fact]
    public void TestUpdateTimes()
    {
        HolePunchInfoPacket clientPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingClient, UInt16.MinValue);
        HolePunchInfoPacket serverPacket = new HolePunchInfoPacket(HolePunchMessageType.StreamingServer, UInt16.MaxValue);
        
        ConnectionPair pair = new ConnectionPair(clientPacket);
        
        pair.SetLastUpdate(clientPacket);
        Assert.True(DateTime.UtcNow.Ticks - pair.LastClientUpdate.Ticks < TimeSpan.TicksPerSecond);
        
        pair.SetLastUpdate(serverPacket);
        Assert.True(DateTime.UtcNow.Ticks - pair.LastServerUpdate.Ticks < TimeSpan.TicksPerSecond);
    }
}
