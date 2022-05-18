﻿using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib;

/// <summary>
/// 
/// </summary>
public class VideoHeaderSink: ITcpSink
{
    private readonly TcpClient client;
    
    /// <inheritdoc />
    public int Port { get; private set; }
    
    /// <inheritdoc />
    public IPAddress Address { get; private set;}
    
    /// <inheritdoc />
    
    
    public bool SendData(byte[] data)
    {
        try
        {
            var stream = client.GetStream();
            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ctor
    /// </summary>
    public VideoHeaderSink()
    {
        Port = 13000;
        Address = IPAddress.Parse("127.0.0.1");
        client = new TcpClient(Address.ToString(), Port);
        
    }
}
