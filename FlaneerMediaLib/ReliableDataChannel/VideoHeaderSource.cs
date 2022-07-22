using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib.ReliableDataChannel;

/// <summary>
/// Receives packets over tcp connection.
/// </summary>
public class VideoHeaderSource: ITcpSource, IDisposable 
{
    /// <summary>
    /// The size of the header in bytes
    /// </summary>
    private const int PacketSize = 5;

    private bool isConnected = true;
        
    private readonly TcpListener listener;
    /// <inheritdoc />
    public int Port { get; private set; }
    /// <inheritdoc />
    public IPAddress Address { get; private set; }

    /// <inheritdoc />
    public event EventHandler<byte[]>? ReceivedData;

    /// <summary>
    /// Ctor
    /// </summary>
    public VideoHeaderSource()
    {
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var broadcastInfo = clas.GetParams(CommandLineArgs.BroadcastAddress);
        Address = IPAddress.Parse(broadcastInfo[0]);
        Port = Int32.Parse(broadcastInfo[1]);
        listener = new TcpListener(Address, Port);
        listener.Start();
        Task.Run(ReceptionThread);
    }

    private void ReceptionThread()
    {
        Byte[] bytes = new Byte[PacketSize];
        
        TcpClient client = listener.AcceptTcpClient();
        while (isConnected)
        {
            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
            stream.Read(bytes, 0, bytes.Length);
       
            // Shutdown and end connection
            client.Close();
            ReceivedData?.Invoke(this, bytes);
        }
    }

    /// <summary>
    /// switches the boolean to false when the object is disposed. 
    /// </summary>
    public void Dispose()
    {
        isConnected = false;
    }
}
