using System.Net;

namespace FlaneerMediaLib;

/// <summary>
/// Interface to define the behaviour of a tcp data channel
/// </summary>
public interface ITcpDataChannel: IService
{
    /// <summary>
    /// port for tcp connections 
    /// </summary>
    int Port { get; }
    /// <summary>
    /// ip address for tcp connections
    /// </summary>
    IPAddress Address { get; }
    
}
