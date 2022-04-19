namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// Interface for all video frame classes to implement
/// </summary>
public interface IVideoFrame
{
    /// <summary>
    /// The codec this frame is encoded in
    /// </summary>
    VideoCodec Codec { get; set; }
    /// <summary>
    /// The width of the frame in pixels
    /// </summary>
    Int16 Width { get; set; }
    /// <summary>
    /// The height of the frame in pixels
    /// </summary>
    Int16 Height { get; set; }
}