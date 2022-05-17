namespace FlaneerMediaLib;

/// <summary>
/// Store of command line arguments
/// </summary>
public class CommandLineArgs
{
    /// <summary>
    /// Arg to use local frames as a video source, should be followed by a path and naming structure.
    /// The naming structure must include a "{}" 
    /// <example>-uselocalframes some/path 1080pTestFrame-{}.h264</example>
    /// </summary>
    public const string UseLocalFrames = "uselocalframes";
}