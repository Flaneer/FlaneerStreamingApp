namespace FlaneerMediaLib;

public class MediaEncoderLifeCycleManager : IDisposable
{
    private IVideoSource videoSource;
    
    public MediaEncoderLifeCycleManager(VideoEncoders videoEncoder)
    {
        switch (videoEncoder)
        {
            case VideoEncoders.NvEncH264:
                videoSource = new NvEncVideoSource();
                ServiceRegistry.AddService(videoSource);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(videoEncoder), videoEncoder, null);
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
