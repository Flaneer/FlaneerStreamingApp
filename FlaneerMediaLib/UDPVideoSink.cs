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
    private byte nextframe = 0;

    public UDPVideoSink(string ip)
    {
        broadcast = IPAddress.Parse(ip);
        s.SendBufferSize = Int16.MaxValue - Utils.UDPHEADERSIZE;
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
                // ignored
            }
        }
        stopWatch.Stop();
    }

    private unsafe void SendFrame(UnmanagedVideoFrame frame)
    {
        using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*) frame.FrameData, frame.FrameSize))
        {
            byte[] frameBytes = new byte[frame.FrameSize];
            ustream.Read(frameBytes, 0, frame.FrameSize);
            
            var frameWritableSize = Int16.MaxValue - Utils.UDPHEADERSIZE;
            var numberOfPackets = (byte) Math.Ceiling((double)frame.FrameSize / frameWritableSize);
            
            IPEndPoint ep = new IPEndPoint(broadcast, 11000);
            byte itCount = 0;
            int sent = 0;
            for (byte i = 0; i < numberOfPackets; i++)
            {
                var frameHeader = new TransmissionVideoFrame
                {
                    Width = (short) videoSource.FrameSettings.Width,
                    Height = (short) videoSource.FrameSettings.Height,
                    NumberOfPackets = numberOfPackets,
                    PacketIdx = itCount,
                    FrameDataSize = frame.FrameSize,
                    SequenceIDX = nextframe
                };
                
                itCount++;
                
                var headerBytes = frameHeader.ToUDPPacket();

                var packetSize = Math.Min(frameWritableSize, frame.FrameSize - sent);
                byte[] transmissionArray = new byte[headerBytes.Length + packetSize];
                Array.Copy(headerBytes, transmissionArray, headerBytes.Length);
                Array.Copy(frameBytes, sent, transmissionArray, headerBytes.Length, packetSize);
                
                s.SendTo(transmissionArray, ep);

                sent += packetSize;
                Console.WriteLine($"SENT CHUNK OF {nextframe} | {sent} / {frame.FrameSize}");
            }
            
            IncrementNextFrame();
        }
    }
    
    private void IncrementNextFrame()
    {
        if (nextframe + 1 > byte.MaxValue)
        {
            nextframe = 0;
        }
        else
        {
            nextframe++;
        }
    }
}