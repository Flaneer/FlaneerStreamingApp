namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// Video frame that uses an pointer to move frame data 
/// </summary>
public class UnmanagedVideoFrame : IVideoFrame
{
    /// <inheritdoc/>
    public VideoCodec Codec { get; set; }
    /// <inheritdoc/>
    public short Width { get; set; }
    /// <inheritdoc/>
    public short Height { get; set; }
    /// <summary>
    /// Pointer to the frame data
    /// </summary>
    public IntPtr FrameData;
    /// <summary>
    /// Size of the frame data
    /// </summary>
    public int FrameSize;
}