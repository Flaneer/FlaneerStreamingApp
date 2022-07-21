using System.Buffers;
using FlaneerMediaLib.Logging;
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
    private Logger logger;


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
        
        var isNotOldFrame = receivedFrame.SequenceIDX >= nextFrameIdx;
        var receivedFrameIsNewIFrame = receivedFrame.IsIFrame && isNotOldFrame;
        if (isNotOldFrame || receivedFrameIsNewIFrame)
        {
            var dataSize = framePacket.Length - TransmissionVideoFrame.HeaderSize;
            var frameData = new byte[dataSize];
            Array.Copy(framePacket, TransmissionVideoFrame.HeaderSize, frameData, 0, dataSize);
            //Print SPS PPS
            /*if (receivedFrameIsNewIFrame)
            {
                ArraySegment<byte> sps = new ArraySegment<byte>(frameData, 4, 21);
                ArraySegment<byte> pps = new ArraySegment<byte>(frameData, 25, 4);
                Console.WriteLine($"SPS: {Convert.ToBase64String(sps)}");
                Console.WriteLine($"PPS: {Convert.ToBase64String(pps)}");
            }*/
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
        if (!frameBuffer.ContainsKey(nextFrameIdx))
        {
            nextFrame = new ManagedVideoFrame();
            return false;
        }

        nextFrame = frameBuffer[nextFrameIdx];

        var lastFrameIdx = nextFrameIdx -1;
        if (frameBuffer.ContainsKey(lastFrameIdx))
            frameBuffer.Remove(lastFrameIdx);
        
        logger.Debug(frameBuffer.Count.ToString());
        
        nextFrameIdx++;
        return true;
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
            if(arr == null)
                return;
            
            totalLength += arr.Length;
        }

        int HeaderSize = (TransmissionVideoFrame.HeaderSize * receivedFrame.NumberOfPackets);
        var frameDataLength = totalLength - HeaderSize;

        var groupedFrameData = new byte[frameDataLength];
        var currentCopiedFrameBytes = 0;
        foreach (var arr in partialFrames[frameSequenceIDX])
        {
            var copySize = arr.Length-TransmissionVideoFrame.HeaderSize;
            Buffer.BlockCopy(arr, TransmissionVideoFrame.HeaderSize, groupedFrameData, currentCopiedFrameBytes, copySize);
            currentCopiedFrameBytes += copySize;
        }

        var frameStream = new MemoryStream(groupedFrameData, 0, groupedFrameData.Length, false, true);
        
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
