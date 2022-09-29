using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Class that handles buffering of frames
/// </summary>
internal class FrameBuffer
{
    private readonly Dictionary<UInt32, PartialFrame?> partialFrames = new();
    private readonly Dictionary<UInt32, ManagedVideoFrame> frameBuffer = new();

    private uint nextFrameIdx;
    
    private readonly VideoCodec codec;
    private readonly Logger logger;

    private int currentSecond = DateTime.Now.Second;
    private int currentSecondBytesIn;

    private int packetCount;
    private long totalBytesIn;

    private bool displayedFirstFrame = false;
    
    /// <summary>
    /// ctor
    /// </summary>
    public FrameBuffer(VideoCodec codec)
    {
        logger = Logger.GetLogger(this);
        this.codec = codec;
    }

    /// <summary>
    /// Adds a frame to the frame buffer
    /// </summary>
    public void BufferFrame(byte[] framePacket)
    {
        TransmissionVideoFrame receivedFrame = TransmissionVideoFrame.FromUDPPacket(framePacket);
        //Bandwidth measurements
        packetCount++;
        if (DateTime.Now.Second == currentSecond)
        {
            currentSecondBytesIn += receivedFrame.PacketSize;
        }
        else
        {
            logger.AmountStat("Bandwidth B/s", currentSecondBytesIn);

            totalBytesIn += currentSecondBytesIn;
            logger.AmountStat("Average Bandwidth B/s", totalBytesIn/packetCount);
            
            currentSecond = DateTime.Now.Second;
            currentSecondBytesIn = 0;
        }
        

        //Check the frame is new, we dont want to do anything with old frames 
        var isOldFrame = receivedFrame.SequenceIDX < nextFrameIdx;
        if(isOldFrame)
            return;

        if (receivedFrame.NumberOfPackets == 1)
            BufferFullFrame(receivedFrame, framePacket);
        else
            BufferPartialFrame(receivedFrame, framePacket);
    }

    /// <summary>
    /// Returns the next frame if it is available
    /// </summary>
    public bool GetNextFrame(out IVideoFrame nextFrame)
    {
        if (!frameBuffer.ContainsKey(nextFrameIdx))
        {
            nextFrame = new ManagedVideoFrame();
            return false;
        }

        nextFrame = frameBuffer[nextFrameIdx];

        var lastFrameIdx = nextFrameIdx -1;
        if (frameBuffer.ContainsKey(lastFrameIdx))
            frameBuffer.Remove(lastFrameIdx);
        
        logger.Debug($"Sending {frameBuffer[nextFrameIdx].Stream!.Length}B Frame From Buffer");
        
        nextFrameIdx++;
        
        if (!displayedFirstFrame)
            displayedFirstFrame = true;
        
        return true;
    }

    private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] framePacket)
    {
        var frameSequenceIDX = receivedFrame.SequenceIDX;
        if(!partialFrames.ContainsKey(frameSequenceIDX))
            partialFrames[frameSequenceIDX] = new PartialFrame(receivedFrame, NewFrameReady);
        partialFrames[frameSequenceIDX]!.BufferPiece(framePacket, receivedFrame.PacketIdx, receivedFrame.PacketSize);
    }

    private void NewFrameReady(uint sequenceIdx, ManagedVideoFrame assembledFrame, bool isIFrame)
    {
        if(isIFrame && displayedFirstFrame)
        {
            logger.Trace($"Skipping to latest I frame: {sequenceIdx}");
            nextFrameIdx = sequenceIdx;
        }
        frameBuffer[sequenceIdx] = assembledFrame;
    }

    private void BufferFullFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
    {
        var frameStream = new MemoryStream(receivedFrame.PacketSize);
        frameStream.Write(frameData, TransmissionVideoFrame.HeaderSize, receivedFrame.PacketSize);

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
