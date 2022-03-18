namespace FlaneerMediaLib;

public class MediaEncoderLifeCycleManager : IDisposable
{
    private IVideoSource videoSource;
    
    public MediaEncoderLifeCycleManager(VideoSources videoSource)
    {
        switch (videoSource)
        {
            case VideoSources.NvEncH264:
                this.videoSource = new NvEncVideoSource();
                ServiceRegistry.AddService(this.videoSource);
                break;
            case VideoSources.UDPH264:
                this.videoSource = new UDPVideoSource(11000);
                ServiceRegistry.AddService<IVideoSource>(this.videoSource);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(videoSource), videoSource, null);
        }
    }

    public bool InitVideo(FrameSettings frameSettings, ICodecSettings codecSettings)
    {
        return videoSource.Init(frameSettings, codecSettings);
    }

    public void Dispose()
    {
        videoSource.Dispose();
    }
}
