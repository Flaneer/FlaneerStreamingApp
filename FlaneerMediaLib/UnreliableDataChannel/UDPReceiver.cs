using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Receives UDP network traffic
/// </summary>
public class UDPReceiver : IService
{
    private IPEndPoint receptionIpEndPoint;
    
    private readonly Dictionary<PacketType, List<Action<SmartBuffer>>> receptionTrafficDestinations = new();
    private bool receiving;
    private readonly Logger logger;
    private readonly UDPClientStatTracker clientStatTracker;
    private readonly SmartBufferManager smartBufferManager;

    private Socket s;
    private readonly CommandLineArgumentStore clas;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPReceiver(Socket s)
    {
        this.s = s;
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService(out clas);
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        
        clientStatTracker = new UDPClientStatTracker(this);
        
        var broadcastInfo = clas.GetParams(CommandLineArgs.BroadcastAddress);
        var listenPort = Int32.Parse(broadcastInfo[1]);
        
        receptionIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        
        ServiceRegistry.TryGetService(out smartBufferManager);

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
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        
        receiving = true;
        while (receiving)
        {
            try
            {
                var endPoint = receptionIpEndPoint as EndPoint;
                var smartBuffer = smartBufferManager.CheckoutNextBuffer();
                s.ReceiveFrom(smartBuffer.Buffer, ref endPoint);

                ProcessReceivedPacket(smartBuffer);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return;
            }
        }
    }

    internal void ProcessReceivedPacket(SmartBuffer smartBuffer)
    {
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        
        var newReceivedByteBuffer = smartBuffer.Buffer; 
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
                    callback(smartBuffer);
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
    public void SubscribeToReceptionTraffic(PacketType packetType, Action<SmartBuffer> callBack)
    {
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        
        if (receptionTrafficDestinations.ContainsKey(packetType))
        {
            receptionTrafficDestinations[packetType].Add(callBack);
        }
        else
        {
            receptionTrafficDestinations.Add(packetType, new List<Action<SmartBuffer>>{callBack});
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        receiving = false;
    }
}
