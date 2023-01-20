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
    
    Dictionary<ushort, ConnectionPair> connections = new();
    private readonly IPEndPoint ipMe;
    private readonly Socket s;
    private readonly Logger logger;
    private bool holePunchingActive = true;

    /// <summary>
    /// ctor
    /// </summary>
    public HolePunchServer()
    {
        logger = LoggerFactory.CreateLogger(this);
        
        s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipMe = new IPEndPoint(IPAddress.Any, 11000);
        s.Bind(ipMe);
    }

    /// <summary>
    /// Begins the reception loop for hole punching
    /// </summary>
    public void InitHolePunching()
    {
        while (holePunchingActive)
        {
            byte[] inBuf = new byte[HolePunchInfoPacket.HeaderSize];
            EndPoint inEP = new IPEndPoint(IPAddress.Any, 11000);
            s.ReceiveFrom(inBuf, ref inEP);
            var inIp = inEP as IPEndPoint ?? throw new InvalidOperationException();
            logger.Info($"Contact from {inIp}");

            var newClient = CreatePacketFromReceptionBuffer(inBuf, inIp);
            logger.Info($"Added new client {newClient}");

            if (AttemptPairing(newClient))
            {
                logger.Info("Connection Established");
            }
            else
            {
                logger.Info($"Client {newClient} registered");
            }
            
            if(connections.TryGetValue(newClient.ConnectionId, out var pair))
                pair.SetLastUpdate(newClient);
            CheckHeartBeats();
        }
    }

    private void CheckHeartBeats()
    {
        foreach (var connection in connections)
        {
            if (connection.Value.LastClientUpdate.AddMilliseconds(HeartbeatInterval * 2) < DateTime.UtcNow)
            {
                logger.Debug($"Connection {connection.Value.Client} timed out");
                connection.Value.RemoveClient(HolePunchMessageType.StreamingClient);
                if (connection.Value.ServerIsConnected)
                    s.SendTo(new HolePunchInfoPacket(HolePunchMessageType.PartnerDisconnected, connection.Key).ToUDPPacket(),
                        connection.Value.Server.ToEndPoint() ?? throw new InvalidOperationException());
            }

            if (connection.Value.LastServerUpdate.AddMilliseconds(HeartbeatInterval * 2) < DateTime.UtcNow)
            {
                logger.Debug($"Connection {connection.Value.Server} timed out");
                connection.Value.RemoveClient(HolePunchMessageType.StreamingServer);
                if(connection.Value.ClientIsConnected)
                    s.SendTo(new HolePunchInfoPacket(HolePunchMessageType.PartnerDisconnected, connection.Key).ToUDPPacket(),
                        connection.Value.Client.ToEndPoint() ?? throw new InvalidOperationException());
            }
        }
    }

    internal bool AttemptPairing(HolePunchInfoPacket newClient)
    {
        if (connections.ContainsKey(newClient.ConnectionId))
        {
            var connectionPair = connections[newClient.ConnectionId];
            //If we make a pair exchange the deets
            if (connectionPair.RegisterClient(newClient))
            {
                s.SendTo(connectionPair.Client.ToUDPPacket(),
                    connectionPair.Server.ToEndPoint() ?? throw new InvalidOperationException());

                s.SendTo(connectionPair.Server.ToUDPPacket(),
                    connectionPair.Client.ToEndPoint() ?? throw new InvalidOperationException());
                return true;
            }
        }
        else
        {
            connections[newClient.ConnectionId] = new ConnectionPair(newClient);
        }
        return false;
    }

    private static HolePunchInfoPacket CreatePacketFromReceptionBuffer(byte[] inBuf, IPEndPoint inIp)
    {
        var packetIn = HolePunchInfoPacket.FromBytes(inBuf);
        return HolePunchInfoPacket.FromEndPoint(inIp, packetIn.HolePunchMessageType, packetIn.ConnectionId);
    }
}
