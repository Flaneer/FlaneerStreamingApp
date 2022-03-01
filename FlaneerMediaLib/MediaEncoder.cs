namespace FlaneerMediaLib
{
    public class MediaEncoder
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
    }
}
