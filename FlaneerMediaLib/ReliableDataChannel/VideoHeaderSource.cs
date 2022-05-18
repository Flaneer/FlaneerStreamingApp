using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib;

/// <summary>
/// Receives packets over tcp connection.
/// </summary>
public class VideoHeaderSource: ITcpSource 
{
    /// <summary>
    /// The size of the header in bytes
    /// </summary>
    private const int PacketSize = 5;
        
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
        Port = 13000;
        Address = IPAddress.Parse("127.0.0.1");
        listener = new TcpListener(Address, Port);
        listener.Start();
        Task.Run(ReceptionThread);
    }

    private void ReceptionThread()
    {
        Byte[] bytes = new Byte[PacketSize];
        
        while (listener.Server.IsBound)
        {
            // Perform a blocking call to accept requests.
            // You could also use server.AcceptSocket() here.
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Connected!");

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
            stream.Read(bytes, 0, bytes.Length);
       
            // Shutdown and end connection
            client.Close();
            ReceivedData?.Invoke(this, bytes);
        }
    }
}
