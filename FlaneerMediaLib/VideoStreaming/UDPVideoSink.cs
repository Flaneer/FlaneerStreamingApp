using System.Diagnostics;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Video sink that broadcasts frames over UDP
/// </summary>
public class UDPVideoSink : IVideoSink
{
    private IEncoder encoder = null!;
    private IVideoSource videoSource = null!;
    private UInt32 nextFrame;

    private readonly UDPSender udpSender;
    
    private readonly Logger logger;
    private readonly VideoSettings videoSettings;
    private int encodeCount;
    private TimeSpan totalEncodeTime;

    private FileStream stream;

    private int sentPacketCount;
    private bool alsoWriteToFile = false;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPVideoSink(VideoSettings videoSettings)
    {
        logger = Logger.GetLogger(this);
        ServiceRegistry.TryGetService(out udpSender);
        
        if(alsoWriteToFile)
            stream = new FileStream("out.h264", FileMode.Create, FileAccess.Write);

        GetEncoder();
        GetSource();
        this.videoSettings = videoSettings;
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
                if (service is IEncoder encoderService)
                    encoder = encoderService;
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
                if (service is IVideoSource sourceService)
                    videoSource = sourceService;
            };
        }
    }
    
    /// <inheritdoc />
    public void ProcessFrame() => CaptureFrameImpl();
    /// <inheritdoc />
    public void ProcessFrames(int numberOfFrames, int targetFramerate) => CaptureFrameImpl(numberOfFrames, targetFramerate);

    private void CaptureFrameImpl(int numberOfFrames = 1, int targetFramerate = -1)
    {
        //Return in the case the encoder is not created
        if(encoder == default! || videoSource == default!)
            return;
        
        while (!udpSender.PeerRegistered)
        {
            logger.Trace("Waiting for peer to be registered");
            Thread.Sleep(100);
        }
        
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        var frameTime = targetFramerate == -1 ? new TimeSpan(0) : new TimeSpan(0, 0, 0, 0, 1000 / targetFramerate);
        
        for (int i = 0; i < numberOfFrames; i++)
        {
            while (stopWatch.Elapsed < (frameTime*i))
            {
                Thread.Sleep(1);
            }

            try
            {
                var startEncodeTime = DateTime.Now;
                //Get a new frame
                var frame = encoder.GetFrame();
                //Stats
                var encodeTime = DateTime.Now - startEncodeTime;
                logger.TimeStat("Encode", encodeTime);
                encodeCount++;
                totalEncodeTime += encodeTime;
                logger.TimeStat("AverageEncodeTime", totalEncodeTime/encodeCount);
                
                if(frame is UnmanagedVideoFrame unmanagedFrame)
                    SendFrame(unmanagedFrame, frameTime);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        stopWatch.Stop();
    }

    private unsafe void SendFrame(UnmanagedVideoFrame frame, TimeSpan frameTime)
    {
        using var uStream = new UnmanagedMemoryStream((byte*) frame.FrameData, frame.FrameSize);
        var frameBytes = new byte[frame.FrameSize];
        uStream.Read(frameBytes, 0, frame.FrameSize);

        WriteToFile(frameBytes);
        
        var numberOfPackets = (byte) Math.Ceiling((double)frame.FrameSize / VideoUtils.FRAMEWRITABLESIZE);

        var sent = 0;
        for (byte i = 0; i < numberOfPackets; i++)
        {
            var frameHeader = new TransmissionVideoFrame
            {
                Width = (short) videoSource.FrameSettings.Width,
                Height = (short) videoSource.FrameSettings.Height,
                NumberOfPackets = numberOfPackets,
                PacketIdx = i,
                FrameDataSize = frame.FrameSize,
                SequenceIDX = nextFrame,
                IsIFrame = nextFrame % videoSettings.GoPLength == 0
            };
            
            var packetSize = Math.Min(VideoUtils.FRAMEWRITABLESIZE, frame.FrameSize - sent);
            var transmissionArraySize = TransmissionVideoFrame.HeaderSize + packetSize;
            
            frameHeader.PacketSize = (ushort) transmissionArraySize;
            var headerBytes = frameHeader.ToUDPPacket();
            
            byte[] transmissionArray = new byte[transmissionArraySize];
            
            Array.Copy(headerBytes, transmissionArray, headerBytes.Length);
            Array.Copy(frameBytes, sent, transmissionArray, headerBytes.Length, packetSize);
                
            udpSender.SendToPeer(transmissionArray);

            sent += packetSize;
            logger.Debug($"SENT CHUNK OF {nextFrame} | {sent} / {frame.FrameSize} [gray]Total Sent Packets = {sentPacketCount++}[/]");
            
            Thread.Sleep(frameTime/numberOfPackets);
        }
        nextFrame++;
    }
    
    private void WriteToFile(byte[] frameBytes)
    {
        if (alsoWriteToFile)
        {
            try
            {
                stream.Write(frameBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
