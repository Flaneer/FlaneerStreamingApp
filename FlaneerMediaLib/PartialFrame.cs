using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib;

internal class PartialFrame
{
    private byte[] frameData;

    private readonly TransmissionVideoFrame seedFrame;

    private int bufferedPieces = 0;

    private Action<uint, ManagedVideoFrame> FrameReadyCallback;

    public PartialFrame(TransmissionVideoFrame seedFrame, Action<uint, ManagedVideoFrame> onFrameReady)
    {
        this.seedFrame = seedFrame;
        FrameReadyCallback = onFrameReady;
        
        frameData = new byte[seedFrame.FrameDataSize];
        frameData.Initialize();
    }
    
    public void BufferPiece(byte[] framePacket, int packetIdx)
    {
        var partialFrameDataLength = framePacket.Length - TransmissionVideoFrame.HeaderSize;
        //This tells us where in the frame to put this part of the frame data
        var partialFrameWriteIDX = packetIdx * VideoUtils.FRAMEWRITABLESIZE;
        Buffer.BlockCopy(framePacket, TransmissionVideoFrame.HeaderSize, frameData, partialFrameWriteIDX, partialFrameDataLength);
        bufferedPieces++;
        if (bufferedPieces == seedFrame.NumberOfPackets)
            AssembleFrame();
    }

    private void AssembleFrame()
    {
        var frameStream = new MemoryStream(frameData, 0, frameData.Length, false, true);
        var assembledFrame = new ManagedVideoFrame()
        {
            Codec = seedFrame.Codec,
            Height = seedFrame.Height,
            Width = seedFrame.Width,
            Stream = frameStream
        };
        FrameReadyCallback(seedFrame.SequenceIDX, assembledFrame);
    }
}
