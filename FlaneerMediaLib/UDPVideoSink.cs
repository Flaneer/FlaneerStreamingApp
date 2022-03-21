using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib;

public class UDPVideoSink : IVideoSink
{
    IEncoder encoder;
    private IVideoSource videoSource;

    private readonly Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private readonly IPAddress broadcast;

    private const int UDPHEADERSIZE = 28;

    public UDPVideoSink(string ip)
    {
        broadcast = IPAddress.Parse(ip);
        GetEncoder();
        GetSource();
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
    
    private void GetSource()
    {
        if (ServiceRegistry.TryGetService<IVideoSource>(out var foundSource))
        {
            videoSource = foundSource;
        }
        else
        {
            ServiceRegistry.ServiceAdded += service =>
            {
                if (service is IVideoSource foundSource)
                    videoSource = foundSource;
            };
        }
    }

    public void CaptureFrame() => CaptureFrameImpl();

    public void CaptureFrames(int numberOfFrames, int targetFramerate) => CaptureFrameImpl(numberOfFrames, targetFramerate);

    private void CaptureFrameImpl(int numberOfFrames = 1, int targetFramerate = -1)
    {
        //Return in the case the encoder is not created
        if(encoder == default || videoSource == default)
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
                if(frame is UnmanagedVideoFrame unmanagedFrame)
                    SendFrame(unmanagedFrame);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        stopWatch.Stop();
    }

    private unsafe void SendFrame(UnmanagedVideoFrame frame)
    {
        using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*) frame.FrameData, frame.FrameSize))
        {
            int n = 0;
            byte itCount = 0;
            //We loop here in case the frame needs to be split into multiple packets
            for (int sent = 0; sent <= ustream.Length; sent+=n)
            {
                var writableSize = Int16.MaxValue - UDPHEADERSIZE;
                var packetSize = Math.Min(writableSize, ustream.Length - sent);
                // Read the source file into a byte array.
                byte[] frameBytes = new byte[packetSize]; //TODO: make space for frame header 
                int numBytesToRead = (int) packetSize;
                // Read may return anything from 0 to numBytesToRead.
                n = ustream.Read(frameBytes, 0, numBytesToRead);
                sent += n;
                IPEndPoint ep = new IPEndPoint(broadcast, 11000);
                
                var frameHeader = new TransmissionVideoFrame
                {
                    Width = videoSource.FrameSettings.Width,
                    Height = videoSource.FrameSettings.Height,
                    IsPartial = sent == ustream.Length,
                    PacketIdx = itCount,
                    FrameDataSize = sent
                };
                itCount++;
                var headerBytes = frameHeader.ToUDPPacket();
                
                byte[] transmissionArray = new byte[headerBytes.Length + frameBytes.Length];
                Array.Copy(headerBytes, transmissionArray, headerBytes.Length);
                Array.Copy(frameBytes, 0, transmissionArray, headerBytes.Length, frameBytes.Length);
                
                s.SendTo(transmissionArray, ep);
                Console.WriteLine($"SENT CHUNK | {sent}/{ustream.Length}");
            }
        }
    }
}