namespace FlaneerMediaLib;

public class ManagedVideoFrame : VideoFrame, IDisposable
{
    
    public MemoryStream Stream = new MemoryStream();

    public void Dispose()
    {
        Stream.Dispose();
    }
}