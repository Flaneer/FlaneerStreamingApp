using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib;

/// <summary>
/// Uses local frames to emulate a UDP video source
/// </summary>
public class LocalFramesVideoSource : IVideoSource
{
    private string framesPath;
    private string frameNameTemplate;

    private string FileNameFromIdx(int idx) => frameNameTemplate.Replace("{}", $"{idx}");

    /// <inheritdoc />
    public ICodecSettings CodecSettings { get; private set; }
    
    /// <inheritdoc />
    public FrameSettings FrameSettings { get; private set; }
    
    /// <inheritdoc />
    public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
    {
        FrameSettings = frameSettingsIn;
        CodecSettings = codecSettingsIn;

        if (!LoadInfoFromCLI())
            return false;

        //TODO: load in files

        return true;
    }

    private bool LoadInfoFromCLI()
    {
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var clParams = clas.GetParams(CommandLineArgs.UseLocalFrames);
        try
        {
            framesPath = clParams[0];
            frameNameTemplate = clParams[1];
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public IVideoFrame GetFrame()
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}