using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace GLDisplayApp;

internal class VideoSettings
{
    public VideoSettings()
    {
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.FrameSettings);
        Width = Int32.Parse(frameSettings[0]);
        Height = Int32.Parse(frameSettings[1]);
    }

    public int Height = 600;
    public int Width = 800;
    public int MaxFPS = 60;
    public BufferFormat Format = BufferFormat.ARGB;
    public int GoPLength = 5;
}
