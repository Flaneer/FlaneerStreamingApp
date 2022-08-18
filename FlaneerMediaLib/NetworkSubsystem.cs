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
        /*var UDPSender = new UDPSender();
        ServiceRegistry.AddService(UDPSender);*/
        
        var UDPReceiver = new UDPReceiver();
        ServiceRegistry.AddService(UDPReceiver);

        /*var ackSender = new AckSender();
        ServiceRegistry.AddService(ackSender);*/
        
        //add measures
        
    }

    /// <summary>
    /// Initialises the network subsystem for the server
    /// </summary>
    public static void InitServer()
    {
        var UDPSender = new UDPSender();
        ServiceRegistry.AddService(UDPSender);

        /*var UDPReceiver = new UDPReceiver();
        ServiceRegistry.AddService(UDPReceiver);*/
        
        /*var ackReceiver = new AckReceiver();
        ServiceRegistry.AddService(ackReceiver);*/
        
        //add control
        var qualityManager = new QualityManager();
        ServiceRegistry.AddService(qualityManager);

    }
}
