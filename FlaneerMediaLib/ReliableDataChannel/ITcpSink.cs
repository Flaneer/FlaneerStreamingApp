namespace FlaneerMediaLib.ReliableDataChannel;

/// <summary>
/// Defines the behaviour of a tcp sink
/// </summary>
public interface ITcpSink : ITcpDataChannel
{
    /// <summary>
    /// sends data to the defined destination 
    /// </summary>
    bool SendData(byte[] data);
}
