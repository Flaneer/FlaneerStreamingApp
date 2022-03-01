using NvEncWrapper;

namespace FlaneerMediaLib
{
    internal class NvEncVideoSource : IVideoSource, IEncoder
    {
        public bool Init(FrameSettings frameSettings, ICodecSettings codecSettings)
        {
            return Wrapper.Init(Utils.FromFrameSettings(frameSettings), Utils.FromCodecSettings(codecSettings));
        }

        public VideoFrame GetFrame()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
