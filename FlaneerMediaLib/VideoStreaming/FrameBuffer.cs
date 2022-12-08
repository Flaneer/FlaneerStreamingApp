using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Class that handles buffering of frames
/// </summary>
public class FrameBuffer
{
    private readonly Dictionary<UInt32, PartialFrame?> partialFrames = new();
    private readonly Dictionary<UInt32, ManagedVideoFrame> frames = new();

    internal int frameBufferCount => frames.Count;
    internal int partialFrameBufferCount => partialFrames.Count;
    
    private uint nextFrameIdx;
    
    private readonly VideoCodec codec;
    private readonly bool fullLogging;
    private readonly Logger logger;

    private int currentSecond = DateTime.Now.Second;
    private int currentSecondBytesIn;

    internal int PacketCount;
    private long totalBytesIn;

    private bool displayedFirstFrame = false;
    
    private SmartBufferManager smartBufferManager;
    private SmartMemoryStreamManager smartMemoryStreamManager;

    /// <summary>
    /// ctor
    /// </summary>
    public FrameBuffer(VideoCodec codec, bool fullLogging = false)
    {
        logger = Logger.GetLogger(this);
        this.codec = codec;
        this.fullLogging = fullLogging;

        if(fullLogging)
            Task.Run(LogFrameBufferInfo);
        
        ServiceRegistry.TryGetService(out smartBufferManager);
        ServiceRegistry.TryGetService(out smartMemoryStreamManager);
    }

    private void LogFrameBufferInfo()
    {
        while (true)
        {
            logger.AmountStat("Frames In Buffer:", frames.Count);
            string frameIdxs = "";
            foreach (var frame in frames)
            {
                frameIdxs += $"{frame.Key}, ";
            }
            logger.Trace("Frame idxs: " + frameIdxs);

            string partials = "";
            foreach (var partialFrame in partialFrames)
            {
                partials += $"{partialFrame.Key}: {partialFrame.Value!.BufferedPieces}/{partialFrame.Value!.ExpectedPieces}, ";
            }
            logger.Trace("Partial idxs: " + partials);
            
            logger.AmountStat("Next Frame:", nextFrameIdx);
            Thread.Sleep(1000);
        }
    }
    
    /// <summary>
    /// Adds a frame to the frame buffer
    /// </summary>
    public void BufferFrame(SmartBuffer framePacket)
    {
        TransmissionVideoFrame receivedFrame = TransmissionVideoFrame.FromUDPPacket(framePacket.Buffer);
        //Bandwidth measurements
        PacketCount++;
        LogStats(receivedFrame.PacketSize);
        
        //Check the frame is new, we dont want to do anything with old frames 
        var isOldFrame = receivedFrame.SequenceIDX < nextFrameIdx;
        if(isOldFrame)
            return;

        if (receivedFrame.NumberOfPackets == 1)
            BufferFullFrame(receivedFrame, framePacket);
        else
            BufferPartialFrame(receivedFrame, framePacket);
    }

    private void LogStats(int packetsize)
    {
        if(!fullLogging)
            return;
        
        if (DateTime.Now.Second == currentSecond)
        {
            currentSecondBytesIn += packetsize;
        }
        else
        {
            logger.AmountStat("Bandwidth B/s", currentSecondBytesIn);

            totalBytesIn += currentSecondBytesIn;
            logger.AmountStat("Average Bandwidth B/s", totalBytesIn / PacketCount);

            currentSecond = DateTime.Now.Second;
            currentSecondBytesIn = 0;
        }
    }

    /// <summary>
    /// Returns the next frame if it is available
    /// </summary>
    public bool GetNextFrame(out IVideoFrame nextFrame)
    {
        if (!frames.ContainsKey(nextFrameIdx))
        {
            nextFrame = new ManagedVideoFrame();
            return false;
        }

        nextFrame = frames[nextFrameIdx];

        var lastFrameIdx = nextFrameIdx -1;
        if (frames.ContainsKey(lastFrameIdx))
            frames.Remove(lastFrameIdx);
        
        logger.Debug($"Sending {frames[nextFrameIdx].Stream!.Length}B Frame From Buffer");
        
        nextFrameIdx++;
        
        if (!displayedFirstFrame)
            displayedFirstFrame = true;
        
        return true;
    }

    internal void BufferPartialFrame(TransmissionVideoFrame receivedFrame, SmartBuffer framePacket)
    {
        var frameSequenceIDX = receivedFrame.SequenceIDX;
        if(!partialFrames.ContainsKey(frameSequenceIDX))
            partialFrames[frameSequenceIDX] = new PartialFrame(receivedFrame, NewFrameReady);
        partialFrames[frameSequenceIDX]!.BufferPiece(framePacket, receivedFrame.PacketIdx, receivedFrame.PacketSize);
    }

    internal void NewFrameReady(uint sequenceIdx, ManagedVideoFrame assembledFrame, bool isIFrame)
    {
        if(isIFrame && displayedFirstFrame)
        {
            logger.Trace($"Skipping to latest I frame: {sequenceIdx}");
            nextFrameIdx = sequenceIdx;
        }
        frames[sequenceIdx] = assembledFrame;
    }

    internal void BufferFullFrame(TransmissionVideoFrame receivedFrame, SmartBuffer frameData)
    {
        var frameStream = smartMemoryStreamManager.GetStream(receivedFrame.PacketSize);
        frameStream.Write(frameData.Buffer, TransmissionVideoFrame.HeaderSize, receivedFrame.PacketSize-TransmissionVideoFrame.HeaderSize);
        smartBufferManager.ReleaseBuffer(frameData);

        var newFrame = new ManagedVideoFrame()
        {
            Codec = codec,
            Height = receivedFrame.Height,
            Width = receivedFrame.Width,
            Stream = frameStream
        };
        NewFrameReady(receivedFrame.SequenceIDX, newFrame, receivedFrame.IsIFrame);
    }
}
