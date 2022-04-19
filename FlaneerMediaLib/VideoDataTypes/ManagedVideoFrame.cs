namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// A frame that uses a (managed) MemoryStream to contain the information
/// </summary>
public class ManagedVideoFrame : IVideoFrame, IDisposable
{
    /// <inheritdoc/>
    public VideoCodec Codec { get; set; }
    /// <inheritdoc/>
    public short Width { get; set; }
    /// <inheritdoc/>
    public short Height { get; set; }
    /// <summary>
    /// Stream to contain the frame data
    /// </summary>
    public MemoryStream Stream = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        Stream.Dispose();
    }
}