using FlaneerMediaLib.VideoDataTypes;
using NvEncWrapper;

namespace FlaneerMediaLib
{
    internal class NvEncVideoSource : IVideoSource, IEncoder
    {
        private FrameSettings frameSettings = null!;
        private ICodecSettings codecSettings = null!;
        private VideoCodec codec;

        public ICodecSettings CodecSettings => codecSettings;

        public FrameSettings FrameSettings => frameSettings;

        public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
        {
            frameSettings = frameSettingsIn;
            codecSettings = codecSettingsIn;
            switch (codecSettingsIn)
            {
                case H264CodecSettings:
                    codec = VideoCodec.H264;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codecSettingsIn));
            }
            return Wrapper.Init(VideoUtils.FromFrameSettings(frameSettingsIn), VideoUtils.FromCodecSettings(codecSettingsIn));
        }

        public bool GetFrame(out IVideoFrame frame)
        {
            frame = GetFrame();
            return true;
        }

        public IVideoFrame GetFrame()
        {
            var frame = Wrapper.RequestNewFrame(frameSettings.Width, frameSettings.Height);
            if (frame.Data == IntPtr.Zero)
                throw new Exception("Invalid frame address provided");
            return new UnmanagedVideoFrame()
            {
                Codec = codec,
                Width = (short) frameSettings.Width,
                Height = (short) frameSettings.Height,
                FrameData = frame.Data,
                FrameSize = frame.BufferSize
            };
        }

        public void Dispose()
        {
            Wrapper.CleanUp();
        }
    }
}
