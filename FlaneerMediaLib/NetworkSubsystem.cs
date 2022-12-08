using System.Net.Sockets;
using FlaneerMediaLib.UnreliableDataChannel;
﻿using FlaneerMediaLib.QualityManagement;

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
        CommonInit();

        var ackSender = new AckSender();
        ServiceRegistry.AddService(ackSender);

        //add measures
    }

    /// <summary>
    /// Initialises the network subsystem for the server
    /// </summary>
    public static void InitServer()
    {
        
        CommonInit();

        var ackReceiver = new AckReceiver();
        ServiceRegistry.AddService(ackReceiver);
        
        //add control
        var qualityManager = new QualityManager();
        ServiceRegistry.AddService(qualityManager);

    }

    private static void CommonInit()
    {
        Socket s = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var udpSender = new UDPSender(s);
        ServiceRegistry.AddService(udpSender);

        var holePunchClient = new HolePunchClient();
        ServiceRegistry.AddService(holePunchClient);

        var udpReceiver = new UDPReceiver(s);
        ServiceRegistry.AddService(udpReceiver);
    }
}
