namespace FlaneerMediaLib
{
    public interface IVideoSource : IService, IDisposable
    {
        public bool Init(FrameSettings frameSettings, ICodecSettings codecSettings);

        VideoFrame GetFrame();
    }
}
