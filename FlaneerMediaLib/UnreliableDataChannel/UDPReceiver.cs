using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Receives UDP network traffic
/// </summary>
public class UDPReceiver : IService
{
    private readonly UdpClient listener;
    private IPEndPoint groupEP;
    
    private Dictionary<PacketType, List<Action<byte[]>>> receptionTrafficDestinations = new();
    private bool receiving;
    private readonly Logger logger;
    private readonly UDPClientStatTracker clientStatTracker;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPReceiver()
    {
        logger = Logger.GetLogger(this);
        clientStatTracker = new UDPClientStatTracker(this);
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var broadcastInfo = clas.GetParams(CommandLineArgs.BroadcastAddress);
        var listenPort = Int32.Parse(broadcastInfo[1]);
        
        listener = new UdpClient(listenPort);
        groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        
        Task.Run(Reception);
    }

    private void Reception()
    {
        receiving = true;
        while (receiving)
        {
            listener.Client.ReceiveBufferSize = Int32.MaxValue;
            try
            {
                var receivedBytes = listener.Receive(ref groupEP);
                if(receivedBytes.Length == 0)
                    continue;

                var receivedType = PacketInfoParser.PacketType(receivedBytes);
                var packetSize = PacketInfoParser.PacketSize(receivedBytes);

                if(packetSize != receivedBytes.Length)
                    logger.Debug($"TransmittedPacketSize:{packetSize}");
                
                if (receptionTrafficDestinations.ContainsKey(receivedType))
                {
                    foreach (var callback in receptionTrafficDestinations[receivedType])
                    {
                        callback(receivedBytes);
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
        listener.Dispose();
    }
}
