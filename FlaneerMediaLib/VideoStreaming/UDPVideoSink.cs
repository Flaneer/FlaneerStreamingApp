using System.Diagnostics;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;
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
    private SmartBufferManager smartBufferManager;
    
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
        
        ServiceRegistry.TryGetService(out smartBufferManager);
        
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
    public void ProcessFrame() => CaptureFrameImpl(1, -1);
    /// <inheritdoc />
    public void ProcessFrames(int numberOfFrames, int targetFramerate = -1) => CaptureFrameImpl(numberOfFrames, targetFramerate);

    private void CaptureFrameImpl(int numberOfFrames, int targetFramerate)
    {
        //Return in the case the encoder is not created
        if(encoder == default! || videoSource == default!)
            return;

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        var frameTime = targetFramerate == -1 ? new TimeSpan(0) : new TimeSpan(0, 0, 0, 0, 1000 / targetFramerate);
        
        for (int i = 0; i < numberOfFrames; i++)
        {
            while (stopWatch.Elapsed < (frameTime*i))
                Thread.Sleep(1);

            try
            {
                var startEncodeTime = DateTime.Now;
                //Get a new frame
                var frame = encoder.GetFrame();
                //Stats
                var encodeTime = DateTime.Now - startEncodeTime;
                logger.TimeStat("EncodeTime", encodeTime);
                encodeCount++;
                totalEncodeTime += encodeTime;
                logger.AmountStat("AverageEncodeTime", (totalEncodeTime/encodeCount).TotalMilliseconds, "ms");
                
                if(frame is UnmanagedVideoFrame unmanagedFrame)
                    SendFrame(unmanagedFrame, frameTime);
            }
            catch (Exception e)
            {
                logger.Error($"Error when sending frame: {e.Message}");
            }
        }
        stopWatch.Stop();
    }
    
    private void SendFrame(UnmanagedVideoFrame frame, TimeSpan frameTime)
    {
        var pixelBuffers = SplitPixels(frame);
        byte pixelBuffersCount = (byte) pixelBuffers.Count;
        for (byte i = 0; i < pixelBuffersCount; i++)
        {
            var now = DateTime.Now;
            var pixelBuffer = pixelBuffers[i];
            var transmissionSize = TransmissionVideoFrame.HeaderSize + pixelBuffer.Length;
            InsertHeader(frame.FrameSize, pixelBuffersCount, i, pixelBuffer.Buffer);
            udpSender.SendToPeer(pixelBuffer.Buffer, transmissionSize);
            
            smartBufferManager.ReleaseBuffer(pixelBuffer);
            
            sentPacketCount++;
        }
        nextFrame++;
    }

    private void InsertHeader(int frameSize, byte packetCount, byte packetIdx, in byte[] pixelBuffer)
    {
        var frameHeader = new TransmissionVideoFrame
        {
            Width = (short) videoSource.FrameSettings.Width,
            Height = (short) videoSource.FrameSettings.Height,
            NumberOfPackets = packetCount,
            PacketIdx = packetIdx,
            PacketSize = (ushort) pixelBuffer.Length,
            FrameDataSize = frameSize,
            SequenceIDX = nextFrame,
            IsIFrame = nextFrame % videoSettings.GoPLength == 0
        };
        frameHeader.ToUDPPacket(pixelBuffer);
    }

    private unsafe List<SmartBuffer> SplitPixels(UnmanagedVideoFrame frame)
    {
        using var uStream = new UnmanagedMemoryStream((byte*) frame.FrameData, frame.FrameSize);
        var frameDataAllocated = 0;
        
        var numberOfPackets = (byte) Math.Ceiling((double)frame.FrameSize / VideoUtils.FRAMEWRITABLESIZE);
        var ret = new List<SmartBuffer>();
        for (int i = 0; i < numberOfPackets; i++)
        {
            var packetSize = Math.Min(VideoUtils.FRAMEWRITABLESIZE, frame.FrameSize - frameDataAllocated);
            var frameDataChunk = smartBufferManager.CheckoutNextBuffer();
            frameDataChunk.Length = packetSize;
            uStream.Read(frameDataChunk.Buffer, TransmissionVideoFrame.HeaderSize, packetSize);
            ret.Add(frameDataChunk);
        }
        return ret;
    }

    /// <summary>
    /// DEBUG CODE
    /// </summary>
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
