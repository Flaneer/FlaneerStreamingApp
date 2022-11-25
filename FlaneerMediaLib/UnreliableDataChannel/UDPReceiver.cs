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

#pragma warning disable CS8618
    /// <summary>
    /// Ctor used for tests
    /// </summary>
    /// <remarks>Are you using this not in a test. Don't. Don't do that.</remarks>
    internal UDPReceiver(){}
#pragma warning restore CS8618

    private void Reception()
    {
        receiving = true;
        while (receiving)
        {
            try
            {
                var endPoint = receptionIpEndPoint as EndPoint;
                s.ReceiveFrom(receivedByteBuffer, ref endPoint);

                ProcessReceivedPacket(receivedByteBuffer);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return;
            }
        }
    }

    internal void ProcessReceivedPacket(byte[] newReceivedByteBuffer)
    {
        if (newReceivedByteBuffer.Length == 0)
            return;

        var receivedType = PacketInfoParser.PacketType(newReceivedByteBuffer);

        if (receivedType == PacketType.HolePunchInfo)
            receptionIpEndPoint = HolePunchInfoPacket.FromBytes(newReceivedByteBuffer).ToEndPoint() ??
                                  throw new InvalidOperationException();

        if (receptionTrafficDestinations.ContainsKey(receivedType))
        {
            try
            {
                foreach (var callback in receptionTrafficDestinations[receivedType])
                {
                    callback(newReceivedByteBuffer);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error with callback of type {receivedType}: {e.ToString()}");
                throw;
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
