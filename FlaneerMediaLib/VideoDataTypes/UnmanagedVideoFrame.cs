namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// Base class for unmanaged frames for both pointer types
/// </summary>
public abstract class BaseUnmanagedVideoFrame : IVideoFrame
{
    /// <inheritdoc/>
    public VideoCodec Codec { get; set; }
    /// <inheritdoc/>
    public short Width { get; set; }
    /// <inheritdoc/>
    public short Height { get; set; }
    /// <summary>
    /// Size of the frame data
    /// </summary>
    public int FrameSize;
}

/// <summary>
/// Video frame that uses an pointer to move frame data 
/// </summary>
public class UnmanagedVideoFrame : BaseUnmanagedVideoFrame
{
    /// <summary>
    /// Pointer to the frame data
    /// </summary>
    public IntPtr FrameData;
}

/// <summary>
/// Video frame that uses an pointer to move frame data 
/// </summary>
public unsafe class UnsafeUnmanagedVideoFrame : BaseUnmanagedVideoFrame
{
    /// <summary>
    /// Pointer to the frame data
    /// </summary>
    public byte* FrameData;
}
