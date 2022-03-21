namespace FlaneerMediaLib
{
    public interface IVideoSource : IService, IDisposable
    {
        ICodecSettings CodecSettings { get; }
        FrameSettings FrameSettings { get; }
        
        public bool Init(FrameSettings frameSettings, ICodecSettings codecSettings);

        VideoFrame GetFrame();
    }
}
