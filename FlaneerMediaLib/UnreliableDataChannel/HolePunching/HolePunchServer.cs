using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel.HolePunching;

/// <summary>
/// The hole punch server allows connections to form without explicit ip sharing
/// </summary>
public class HolePunchServer : IDisposable
{
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
                connections.Remove(newClient.ConnectionId);
            }
            else
            {
                logger.Info($"Client {newClient} registered");
                //Heartbeat code goes here
            }
        }
    }

    private bool AttemptPairing(HolePunchInfoPacket newClient)
    {
        if (connections.ContainsKey(newClient.ConnectionId))
        {
            var connectionPair = connections[newClient.ConnectionId];
            connectionPair.RegisterClient(newClient);

            s.SendTo(connectionPair.Client.ToUDPPacket(),
                connectionPair.Server.ToEndPoint() ?? throw new InvalidOperationException());

            s.SendTo(connectionPair.Server.ToUDPPacket(),
                connectionPair.Client.ToEndPoint() ?? throw new InvalidOperationException());

            return true;
        }
        connections[newClient.ConnectionId] = new ConnectionPair(newClient);
        return false;
    }

    private static HolePunchInfoPacket CreatePacketFromReceptionBuffer(byte[] inBuf, IPEndPoint inIp)
    {
        var packetIn = HolePunchInfoPacket.FromBytes(inBuf);
        return HolePunchInfoPacket.FromIpEndpoint(inIp, packetIn.NodeType, packetIn.ConnectionId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        holePunchingActive = false;
    }
}
