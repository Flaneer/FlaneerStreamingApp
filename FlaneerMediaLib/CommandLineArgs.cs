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
}