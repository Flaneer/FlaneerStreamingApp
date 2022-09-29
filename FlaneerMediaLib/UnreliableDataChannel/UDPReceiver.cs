using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Receives UDP network traffic
/// </summary>
public class UDPReceiver : IService
{
    private IPEndPoint receptionIpEndPoint;
    
    private readonly Dictionary<PacketType, List<Action<byte[]>>> receptionTrafficDestinations = new();
    private bool receiving;
    private readonly Logger logger;
    private readonly UDPClientStatTracker clientStatTracker;

    private Socket s;
    private byte[] receivedByteBuffer = new byte[Int16.MaxValue];

    /// <summary>
    /// ctor
    /// </summary>
    public UDPReceiver(Socket s)
    {
        this.s = s;
        logger = Logger.GetLogger(this);
        clientStatTracker = new UDPClientStatTracker(this);
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var broadcastInfo = clas.GetParams(CommandLineArgs.BroadcastAddress);
        var listenPort = Int32.Parse(broadcastInfo[1]);
        
        receptionIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        
        //s.Bind(receptionIpEndPoint);

        Task.Run(Reception);
    }

    private void Reception()
    {
        receiving = true;
        while (receiving)
        {
            try
            {
                var endPoint = receptionIpEndPoint as EndPoint;
                s.ReceiveFrom(receivedByteBuffer, ref endPoint);
                
                logger.Trace($"Received packet from {endPoint}");
                
                if(receivedByteBuffer.Length == 0)
                    continue;

                var receivedType = PacketInfoParser.PacketType(receivedByteBuffer);

                if (receivedType == PacketType.HolePunchInfo)
                    receptionIpEndPoint = HolePunchInfoPacket.FromBytes(receivedByteBuffer).ToEndPoint() ?? throw new InvalidOperationException();
                
                var packetSize = PacketInfoParser.PacketSize(receivedByteBuffer);

                if(packetSize != receivedByteBuffer.Length)
                    logger.Debug($"TransmittedPacketSize:{packetSize}");
                
                if (receptionTrafficDestinations.ContainsKey(receivedType))
                {
                    try
                    {
                        foreach (var callback in receptionTrafficDestinations[receivedType])
                        {
                            callback(receivedByteBuffer);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error with callback of type {receivedType}: {e.ToString()}");
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return;
            }
        }
    }

    /// <summary>
    /// Calls the given method when a packet of the given type is received
    /// </summary>
    public void SubscribeToReceptionTraffic(PacketType packetType, Action<byte[]> callBack)
    {
        if (receptionTrafficDestinations.ContainsKey(packetType))
        {
            receptionTrafficDestinations[packetType].Add(callBack);
        }
        else
        {
            receptionTrafficDestinations.Add(packetType, new List<Action<byte[]>> {callBack});
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        receiving = false;
    }
}
