using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib;

/// <summary>
/// Controls the lifecycle (creation/deletion) of media encoders
/// <remarks>This is named "media" since it should handle both video and audio</remarks>
/// </summary>
public class MediaEncoderLifeCycleManager : IDisposable
{
    private readonly IVideoSource videoSource;
    
    /// <summary>
    /// ctor
    /// </summary>
    public MediaEncoderLifeCycleManager(VideoSource videoSource)
    {
        switch (videoSource)
        {
            case VideoSource.NvEncH264:
                this.videoSource = new NvEncVideoSource();
                ServiceRegistry.AddService(this.videoSource);
                break;
            case VideoSource.UDPH264:
                this.videoSource = new UDPVideoSource(11000);
                ServiceRegistry.AddService(this.videoSource);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(videoSource), videoSource, null);
        }
    }

    /// <summary>
    /// Initialises the video source
    /// </summary>
    public bool InitVideoSource(FrameSettings frameSettings, ICodecSettings codecSettings)
    {
        return videoSource.Init(frameSettings, codecSettings);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        videoSource.Dispose();
    }
}
