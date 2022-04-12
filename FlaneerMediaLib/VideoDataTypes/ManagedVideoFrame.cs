namespace FlaneerMediaLib.VideoDataTypes;

public class ManagedVideoFrame : VideoFrame, IDisposable
{
    
    public MemoryStream Stream = new();

    public void Dispose()
    {
        Stream.Dispose();
    }
}