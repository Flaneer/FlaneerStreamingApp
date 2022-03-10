using NvEncWrapper;

namespace FlaneerMediaLib
{
    internal class NvEncVideoSource : IVideoSource, IEncoder
    {
        private FrameSettings frameSettings;
        private ICodecSettings codecSettings;
        private VideoCodec codec;

        public bool Init(FrameSettings frameSettings, ICodecSettings codecSettings)
        {
            this.frameSettings = frameSettings;
            this.codecSettings = codecSettings;
            switch (codecSettings)
            {
                case H264CodecSettings:
                    codec = VideoCodec.H264;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codecSettings));
            }
            return Wrapper.Init(Utils.FromFrameSettings(frameSettings), Utils.FromCodecSettings(codecSettings));
        }

        public VideoFrame GetFrame()
        {
            var frame = Wrapper.RequestNewFrame(frameSettings.Width, frameSettings.Height);
            if (frame.Data == IntPtr.Zero)
                throw new Exception("Invalid frame address provided");
            return new VideoFrame()
            {
                Codec = codec,
                Width = frameSettings.Width,
                Height = frameSettings.Height,
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
