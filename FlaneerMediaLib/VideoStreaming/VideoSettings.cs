using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib;

/// <summary>
/// Class to parse the video settings from the command line
/// </summary>
public class VideoSettings
{
    /// <summary>
    /// ctor
    /// </summary>
    public VideoSettings()
    {
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.FrameSettings);
        Width = Int32.Parse(frameSettings[0]);
        Height = Int32.Parse(frameSettings[1]);
    }

    /// <summary>
    /// The height of the video in pixels
    /// </summary>
    public int Height = 600;
    /// <summary>
    /// The width of the video in pixels
    /// </summary>
    public int Width = 800;
    /// <summary>
    /// The framerate cap
    /// </summary>
    public int MaxFPS = 60;
    /// <summary>
    /// The pixel format of the video
    /// </summary>
    public BufferFormat Format = BufferFormat.ARGB;
    /// <summary>
    /// The group of pictures length
    /// </summary>
    public int GoPLength = 5;
}
