namespace FlaneerMediaLib;

/// <summary>
/// Store of command line arguments
/// </summary>
public class CommandLineArgs
{
    /// <summary>
    /// Arg to use local frames as a video source, should be followed by a path, naming structure and a number of frames.
    /// The naming structure must include a "{}. The number of frames assumes index staring at 0" 
    /// <example>-uselocalframes some/path 1080pTestFrame-{}.h264 100</example>
    /// </summary>
    public const string UseLocalFrames = "uselocalframes";

    /// <summary>
    /// The size of the frame in pixels, provide the width then the height
    /// <example>-framesize 1920 1080</example>
    /// </summary>
    public const string FrameSettings = "framesize";

    /// <summary>
    /// The IP address to broadcast to followed by the port
    /// <example>-broadcastip 127.0.0.1 8000</example>
    /// </summary>
    public const string BroadcastAddress = "broadcastaddress";
}
