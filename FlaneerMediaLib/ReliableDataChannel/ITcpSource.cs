namespace FlaneerMediaLib.ReliableDataChannel;

/// <summary>
/// Defines the behaviour of a tcp source
/// </summary>
public interface ITcpSource : ITcpDataChannel
{
    /// <summary>
    /// Receives any available data   
    /// </summary>
    event EventHandler<byte[]> ReceivedData;
}
