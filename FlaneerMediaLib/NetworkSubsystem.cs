namespace FlaneerMediaLib;

/// <summary>
/// Static class that can be used in the main to start all the network services
/// </summary>
public static class NetworkSubsystem
{
    /// <summary>
    /// Initialises the network subsystem for the client
    /// </summary>
    public static void InitClient()
    {
        var UDPReceiver = new UDPReceiver();
        ServiceRegistry.AddService(UDPReceiver);

        var UDPSender = new UDPSender();
        ServiceRegistry.AddService(UDPSender);

        var ackSender = new AckSender();
        ServiceRegistry.AddService(ackSender);
    }

    /// <summary>
    /// Initialises the network subsystem for the server
    /// </summary>
    public static void InitServer()
    {
        
    }
}
