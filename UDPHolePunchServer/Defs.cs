using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace UDPHolePunchServer;

[StructLayout(LayoutKind.Sequential)]
public struct Client
{
    public UInt32 host;
    public UInt16 port;

    public byte[] ToBytes()
    {
        byte[] ret = new byte[6];
        using MemoryStream stream = new MemoryStream(ret);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write(host);
        writer.Write(port);
        return ret;
    }

    public static Client FromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, 0, 6);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);
        var ret = new Client()
        {
            host = reader.ReadUInt32(),
            port = reader.ReadUInt16(),
        };
        
        return ret;
    }

    public static Client FromIpEndpoint(IPEndPoint ep)
    {
        return new Client()
        {
            host = Utils.IpToUInt32(ep.Address.ToString()),
            port = (UInt16) ep.Port
        };
    }

    public IPEndPoint ToEndPoint() => new IPEndPoint(new IPAddress(host), port);

    public override string ToString() => $"{new IPAddress(host)}:{port}";
};

public static class Utils
{
    public static UInt32 IpToUInt32(string ip)
    {
        if (!IPAddress.TryParse(ip, out IPAddress address)) return 0;
        return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
    }
}


