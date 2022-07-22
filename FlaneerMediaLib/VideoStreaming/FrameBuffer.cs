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

        //Check the frame is new, we dont want to do anything with old frames 
        var isOldFrame = receivedFrame.SequenceIDX < nextFrameIdx;
        if(isOldFrame)
            return;
        
        //Check if the frame is a new I frame, in which case we skip to it to reduce latency
        var receivedFrameIsNewIFrame = receivedFrame.IsIFrame && !isOldFrame;
        if(receivedFrameIsNewIFrame)
        {
            logger.Trace($"Skipping to latest I frame: {receivedFrame.SequenceIDX}");
            nextFrameIdx = receivedFrame.SequenceIDX;
        }

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
        
        logger.Debug($"Sending {frameBuffer[nextFrameIdx].Stream.Length}B Frame From Buffer");
        
        nextFrameIdx++;
        return true;
    }

    private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] framePacket)
    {
        var frameSequenceIDX = receivedFrame.SequenceIDX;
        if(!partialFrames.ContainsKey(frameSequenceIDX))
            partialFrames[frameSequenceIDX] = new PartialFrame(receivedFrame, OnPartialFrameAssembled);
        partialFrames[frameSequenceIDX]!.BufferPiece(framePacket, receivedFrame.PacketIdx);
    }

    private void OnPartialFrameAssembled(uint sequenceIdx, ManagedVideoFrame assembledFrame) => frameBuffer[sequenceIdx] = assembledFrame;

    private void BufferFullFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
    {
        var frameDataLength = frameData.Length - TransmissionVideoFrame.HeaderSize;
        var frameStream = new MemoryStream(frameDataLength);
        frameStream.Write(frameData, TransmissionVideoFrame.HeaderSize, frameDataLength);
            
        frameBuffer[receivedFrame.SequenceIDX] = new ManagedVideoFrame()
        {
            Codec = codec,
            Height = receivedFrame.Height,
            Width = receivedFrame.Width,
            Stream = frameStream 
        };
    }
}
