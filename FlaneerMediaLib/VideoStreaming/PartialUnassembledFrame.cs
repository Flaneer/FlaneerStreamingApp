using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

internal class PartialUnassembledFrame : UnassembledFrame
{
    private Dictionary<int, SmartBuffer> framePieces;
    private readonly int[] order;

    public PartialUnassembledFrame(VideoCodec codec, TransmissionVideoFrame frame, Dictionary<int, SmartBuffer> framePieces, int[] order)
    {
        this.codec = codec;
        this.frame = frame;
        this.framePieces = framePieces;
        this.order = order;

        ServiceRegistry.TryGetService(out smartBufferManager);
        ServiceRegistry.TryGetService(out smartMemoryStreamManager);
    }

    public override ManagedVideoFrame ToFrame()
    {
        var frameStream = smartMemoryStreamManager.GetStream(frame.PacketSize);
        return AssembleFrameImpl(frameStream, frame, order, framePieces, smartBufferManager);
    }
    
    internal static ManagedVideoFrame AssembleFrameImpl(MemoryStream stream, TransmissionVideoFrame seedFrame, int[] order, Dictionary<int, SmartBuffer> framePieces,  SmartBufferManager? smartBufferManager = null)
    {
        foreach (var idx in order)
        {
            var framePiece = framePieces[idx];
            stream.Write(framePiece.Buffer);
            smartBufferManager?.ReleaseBuffer(framePiece);
        }

        var assembledFrame = new ManagedVideoFrame()
        {
            Codec = seedFrame.Codec,
            Height = seedFrame.Height,
            Width = seedFrame.Width,
            Stream = stream
        };
        return assembledFrame;
    }
}
