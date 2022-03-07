using NvEncWrapper;

namespace FlaneerMediaLib
{
    public class MediaEncoder : IDisposable
    {
        public MediaEncoder(VideoEncoders videoEncoder)
        {
            switch (videoEncoder)
            {
                case VideoEncoders.NvEncH264:
                    ServiceRegistry.AddService(new NvEncVideoSource());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(videoEncoder), videoEncoder, null);
            }
        }

        public bool InitVideo(FrameSettings frameSettings, ICodecSettings codecSettings)
        {
            if (ServiceRegistry.TryGetService<IVideoSource>(out var videoSource))
            {
                return videoSource.Init(frameSettings, codecSettings);
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (ServiceRegistry.TryGetService<IVideoSource>(out var videoSource))
            {
                videoSource.Dispose();
            }
        }
    }
}
