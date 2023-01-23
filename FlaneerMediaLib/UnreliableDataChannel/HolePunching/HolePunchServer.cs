using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel.HolePunching;

/// <summary>
/// The hole punch server allows connections to form without explicit ip sharing
/// </summary>
public class HolePunchServer : IService
{
    /// <summary>
    /// The interval in ms to send a keep alive packet
    /// </summary>
    public const int HeartbeatInterval = 5000;
    
    private Dictionary<ushort, ConnectionPair> connections = new();
    private readonly IPEndPoint ipMe;
    private readonly Socket s;
    private readonly Logger logger;
    private bool holePunchingActive = true;
    private List<ushort> connectionsToRemove;
    private bool noNet;

    /// <summary>
    /// ctor
    /// </summary>
    public HolePunchServer()
    {
        logger = LoggerFactory.CreateLogger(this);
        
        s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipMe = new IPEndPoint(IPAddress.Any, 11000);
        s.Bind(ipMe);

        Task.Run(InitHolePunching);
        CheckHeartBeats();
    }

    /// <summary>
    /// Test ctor
    /// <remarks>Don't use this in production code... why would you want a nonet server in prod</remarks>
    /// </summary>
    internal HolePunchServer(bool noNet)
    {
        this.noNet = noNet;
        logger = LoggerFactory.CreateLogger(this);
    }
    
    /// <summary>
    /// Begins the reception loop for hole punching
    /// </summary>
    private void InitHolePunching()
    {
        while (holePunchingActive)
        {
            byte[] inBuf = new byte[HolePunchInfoPacket.HeaderSize];
            EndPoint inEP = new IPEndPoint(IPAddress.Any, 11000);
            s.ReceiveFrom(inBuf, ref inEP);
            var inIp = inEP as IPEndPoint ?? throw new InvalidOperationException();
            logger.Info($"Contact from {inIp}");

            var newClient = CreatePacketFromReceptionBuffer(inBuf, inIp);
            
            lock (connections)
            {
                if (AttemptPairing(newClient))
                {
                    logger.Info("Connection Established");
                }
            }
        }
    }

    private void CheckHeartBeats()
    {
        while (holePunchingActive)
        {
            try
            {
                lock (connections)
                {
                    CheckAllConnections();
                }
            }
            catch
            {
                // ignored
            }
        }
        Thread.Sleep(500);
    }

    private void CheckAllConnections()
    {
        connectionsToRemove = new List<ushort>();
        foreach (var connection in connections)
        {
            if (connection.Value.ClientIsConnected && connection.Value.LastClientUpdate.AddMilliseconds(HeartbeatInterval * 2) < DateTime.UtcNow)
            {
                logger.Debug( $"Connection {connection.Value.Client} timed out, last update was {DateTime.UtcNow - connection.Value.LastClientUpdate} ago");
                connection.Value.RemoveClient(HolePunchMessageType.StreamingClient);
                if (connection.Value.ServerIsConnected)
                {
                    var buffer = new HolePunchInfoPacket(HolePunchMessageType.PartnerDisconnected, connection.Key).ToUDPPacket();
                    SendTo(buffer, connection.Value.Server.ToEndPoint() ?? throw new InvalidOperationException());
                }
            }

            if (connection.Value.ServerIsConnected && connection.Value.LastServerUpdate.AddMilliseconds(HeartbeatInterval * 2) < DateTime.UtcNow)
            {
                logger.Debug($"Connection {connection.Value.Server} timed out, last update was {DateTime.UtcNow - connection.Value.LastServerUpdate} ago");
                connection.Value.RemoveClient(HolePunchMessageType.StreamingServer);
                if (connection.Value.ClientIsConnected)
                {
                    var buffer = new HolePunchInfoPacket(HolePunchMessageType.PartnerDisconnected, connection.Key).ToUDPPacket();
                    SendTo(buffer, connection.Value.Client.ToEndPoint() ?? throw new InvalidOperationException());
                }
            }
            
            if (!connection.Value.ClientIsConnected && !connection.Value.ServerIsConnected)
            {
                connectionsToRemove.Add(connection.Key);
            }
        }

        foreach (var connectionId in connectionsToRemove)
        {
            connections.Remove(connectionId);
        }
    }

    internal bool AttemptPairing(HolePunchInfoPacket newClient)
    {
        if (connections.TryGetValue(newClient.ConnectionId, out var connectionPair))
        {
            //If we make a NEW pair exchange the deets
            if (connectionPair.RegisterClient(newClient))
            {
                try
                {
                    SendTo(connectionPair.Client.ToUDPPacket(), connectionPair.Server.ToEndPoint() ?? throw new InvalidOperationException());
                    SendTo(connectionPair.Server.ToUDPPacket(), connectionPair.Client.ToEndPoint() ?? throw new InvalidOperationException());
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }
        }
        else
        {
            var newConnectionPair = new ConnectionPair(newClient);
            connections[newClient.ConnectionId] = newConnectionPair;
        }
        return false;
    }

    private void SendTo(byte[] buffer, EndPoint endPoint)
    {
        if(noNet)
            return;
        s.SendTo(buffer, endPoint);
    }
    
    private static HolePunchInfoPacket CreatePacketFromReceptionBuffer(byte[] inBuf, IPEndPoint inIp)
    {
        var packetIn = HolePunchInfoPacket.FromBytes(inBuf);
        return HolePunchInfoPacket.FromEndPoint(inIp, packetIn.HolePunchMessageType, packetIn.ConnectionId);
    }
}
