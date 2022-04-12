using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
{
    public interface IVideoSource : IService, IDisposable
    {
        ICodecSettings CodecSettings { get; }
        FrameSettings FrameSettings { get; }
        
        public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn);

        VideoFrame GetFrame();
    }
}
