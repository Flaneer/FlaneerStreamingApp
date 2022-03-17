using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib;

public class UDPVideoSink : IVideoSink
{
    IEncoder encoder;

    private readonly Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private readonly IPAddress broadcast;

    private const int UDPHEADERSIZE = 28;

    public UDPVideoSink(string ip)
    {
        broadcast = IPAddress.Parse(ip);
        GetEncoder();
    }

    private void GetEncoder()
    {
        if (ServiceRegistry.TryGetService<IEncoder>(out var foundEncoder))
        {
            encoder = foundEncoder;
        }
        else
        {
            ServiceRegistry.ServiceAdded += service =>
            {
                if (service is IEncoder foundEncoder)
                    encoder = foundEncoder;
            };
        }
    }

    public void CaptureFrame() => CaptureFrameImpl();

    public void CaptureFrames(int numberOfFrames, int targetFramerate) => CaptureFrameImpl(numberOfFrames, targetFramerate);

    private void CaptureFrameImpl(int numberOfFrames = 1, int targetFramerate = -1)
    {
        //Return in the case the encoder is not created
        if(encoder == default)
            return;
        
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        var frameTime = targetFramerate == -1 ? new TimeSpan(0) : new TimeSpan(0, 0, (int)Math.Floor(1.0f/ targetFramerate));

        for (int i = 0; i < numberOfFrames; i++)
        {
            while (stopWatch.Elapsed < (frameTime*i))
            {
                Thread.Sleep(1);
            }

            try
            {
                var frame = encoder.GetFrame();
                SendFrame(frame);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        stopWatch.Stop();
    }

    private unsafe void SendFrame(VideoFrame frame)
    {
        using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*) frame.FrameData, frame.FrameSize))
        {
            int n = 0;
            //We loop here in case the frame needs to be split into multiple packets
            for (int sent = 0; sent <= ustream.Length; sent+=n)
            {
                var bodySize = Int16.MaxValue - UDPHEADERSIZE;
                var packetSize = Math.Min(bodySize, ustream.Length - sent);
                // Read the source file into a byte array.
                byte[] bytes = new byte[packetSize];
                int numBytesToRead = (int) packetSize;
                // Read may return anything from 0 to numBytesToRead.
                n = ustream.Read(bytes, 0, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                sent += n;

                IPEndPoint ep = new IPEndPoint(broadcast, 11000);
                s.SendTo(bytes, ep);
                Console.WriteLine($"SENT CHUNK | {sent}/{ustream.Length}");
            }
        }
    }
}