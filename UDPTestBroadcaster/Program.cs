using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static string pathSource = @"C:\Users\Tom\Code\FlaneerStreamingApp\TestResources\Netflix_RollerCoaster_4096x2160_60fps_10bit_420-gpu-0.mp4";
    
    static void Main(string[] args)
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress broadcast = IPAddress.Parse("192.168.1.255");

        int sent = 0;
        
        using (FileStream fsSource = new FileStream(pathSource, FileMode.Open, FileAccess.Read))
        {
            while (sent <= fsSource.Length)
            {
                var bodySize = 65507;
                var packetSize = Math.Min(bodySize, fsSource.Length - sent);
                // Read the source file into a byte array.
                byte[] bytes = new byte[packetSize];
                int numBytesToRead = (int)packetSize;
                // Read may return anything from 0 to numBytesToRead.
                int n = fsSource.Read(bytes, 0, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                sent += n;
            
                IPEndPoint ep = new IPEndPoint(broadcast, 11000);
                s.SendTo(bytes, ep);
                Console.WriteLine($"SENT CHUNK | {sent}/{fsSource.Length}");
            }
        }
        
        Console.WriteLine("Message sent to the broadcast address");
    }
}