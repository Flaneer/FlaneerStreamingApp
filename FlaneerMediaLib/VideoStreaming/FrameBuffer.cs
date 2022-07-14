using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Class that handles buffering of frames
/// </summary>
internal class FrameBuffer
{
    private readonly Dictionary<UInt32, byte[][]> partialFrames = new();
    private readonly Dictionary<UInt32, ManagedVideoFrame> frameBuffer = new();

    private uint nextFrameIdx = 0;
    
    private VideoCodec codec;

    /// <summary>
    /// ctor
    /// </summary>
    public FrameBuffer(VideoCodec codec)
    {
        this.codec = codec;
    }

    /// <summary>
    /// Adds a frame to the frame buffer
    /// </summary>
    public void BufferFrame(byte[] framePacket)
    {
        TransmissionVideoFrame receivedFrame = TransmissionVideoFrame.FromUDPPacket(framePacket);
        
        var isNotOldFrame = receivedFrame.SequenceIDX >= nextFrameIdx;
        var receivedFrameIsNewIFrame = receivedFrame.IsIFrame && isNotOldFrame;
        if (isNotOldFrame || receivedFrameIsNewIFrame)
        {
            var dataSize = framePacket.Length - TransmissionVideoFrame.HeaderSize;
            var frameData = new byte[dataSize];
            Array.Copy(framePacket, TransmissionVideoFrame.HeaderSize, frameData, 0, dataSize);
        }
        else
        {
            return;
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
        if (frameBuffer.ContainsKey(nextFrameIdx))
        {
            nextFrame = frameBuffer[nextFrameIdx];
            nextFrameIdx++;
            return true;
        }
        
        nextFrame = new ManagedVideoFrame();
        return false;
    }

    private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
    {
        var frameSequenceIDX = receivedFrame.SequenceIDX;
        if (!partialFrames.ContainsKey(frameSequenceIDX))
            partialFrames[frameSequenceIDX] = new byte[receivedFrame.NumberOfPackets][];

        partialFrames[frameSequenceIDX][receivedFrame.PacketIdx] = frameData;
        var totalLength = 0;
        foreach (var arr in partialFrames[frameSequenceIDX])
        {
            var len = arr.Length;
            if (len != 0)
                totalLength += len;
            else
                return;
        }
        var frameDataLength = totalLength - (TransmissionVideoFrame.HeaderSize * receivedFrame.NumberOfPackets);
        var frameStream = new MemoryStream(frameDataLength);
        foreach (var arr in partialFrames[frameSequenceIDX])
        {
            frameStream.Write(arr, TransmissionVideoFrame.HeaderSize, arr.Length - TransmissionVideoFrame.HeaderSize);
        }
        frameBuffer[receivedFrame.SequenceIDX] = new ManagedVideoFrame()
        {
            Codec = codec,
            Height = receivedFrame.Height,
            Width = receivedFrame.Width,
            Stream = frameStream 
        };
    }
    
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
