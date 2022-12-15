using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

internal class UnassembledFrame : IDisposable
{
    protected VideoCodec codec;
    protected TransmissionVideoFrame frame;
    protected SmartMemoryStreamManager smartMemoryStreamManager;
    protected SmartBufferManager smartBufferManager;
    
    private readonly SmartBuffer frameData;

    protected UnassembledFrame(){}

    public UnassembledFrame(VideoCodec codec, TransmissionVideoFrame frame, SmartBuffer frameData)
    {
        this.codec = codec;
        this.frame = frame;
        this.frameData = frameData;
        
        ServiceRegistry.TryGetService(out smartBufferManager);
        ServiceRegistry.TryGetService(out smartMemoryStreamManager);
    }
    
    public virtual ManagedVideoFrame ToFrame()
    {
        var frameStream = smartMemoryStreamManager.GetStream(frame.PacketSize);
        frameStream.Write(frameData.Buffer, TransmissionVideoFrame.HeaderSize, frame.PacketSize-TransmissionVideoFrame.HeaderSize);
        smartBufferManager.ReleaseBuffer(frameData);

        return new ManagedVideoFrame()
        {
            Codec = codec,
            Height = frame.Height,
            Width = frame.Width,
            Stream = frameStream
        };
    }

    public void Dispose()
    {
        smartBufferManager.ReleaseBuffer(frameData);
    }
}
