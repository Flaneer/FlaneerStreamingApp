namespace FlaneerMediaLib.VideoDataTypes;


/// <summary>
/// Information needed to generate a frame
/// </summary>
public class FrameSettings
{
    /// <summary>
    /// The width of the frame in pixels
    /// </summary>
    public int Width;
    /// <summary>
    /// The height of the frame in pixels
    /// </summary>
    public int Height;
    /// <summary>
    /// The FPS cap of the frame, drives the delay
    /// </summary>
    public int MaxFPS;
}