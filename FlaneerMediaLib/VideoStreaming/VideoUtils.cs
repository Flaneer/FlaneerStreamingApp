using FlaneerMediaLib.VideoDataTypes;
using NvEncWrapper;

namespace FlaneerMediaLib.VideoStreaming
{
    internal class VideoUtils
    {
        /// <summary>
        /// 8 plus the current UDP header size
        /// </summary>
        public const int UDPHEADERSIZE = 8 + TransmissionVideoFrame.HeaderSize;

        /// <summary>
        /// The size of the UDP packet with space for the header
        /// </summary>
        public const int FRAMEWRITABLESIZE = Int16.MaxValue - VideoUtils.UDPHEADERSIZE;
        
        public static NvEncWrapper.H264CodecSettings FromCodecSettings(ICodecSettings settings)
        {
            if (settings is H264CodecSettings h264CodecSettings)
            {
                return new NvEncWrapper.H264CodecSettings
                {
                    Format = FromBufferFormat(h264CodecSettings.Format),
                    GoPLength = h264CodecSettings.GoPLength
                };
            }
            else
            {
                throw new Exception($"Trying to construct H264 Codec settings with {settings.GetType()}");
            }
        }

        public static VideoCaptureSettings FromFrameSettings(FrameSettings frameSettings)
        {
            return new VideoCaptureSettings
            {
                Height = (Int16)frameSettings.Height,
                Width = (Int16)frameSettings.Width,
                MaxFPS = (Int16)frameSettings.MaxFPS
            };
        }

        private static NV_BufferFormat FromBufferFormat(BufferFormat bufferFormat)
        {
            switch (bufferFormat)
            {
                case BufferFormat.UNDEFINED:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_UNDEFINED;
                case BufferFormat.NV12:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_NV12;
                case BufferFormat.YV12:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_YV12;
                case BufferFormat.IYUV:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_IYUV;
                case BufferFormat.YUV444:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_YUV444;
                case BufferFormat.YUV420_10BIT:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_YUV420_10BIT;
                case BufferFormat.YUV444_10BIT:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_YUV444_10BIT;
                case BufferFormat.ARGB:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_ARGB;
                case BufferFormat.ARGB10:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_ARGB10;
                case BufferFormat.AYUV:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_AYUV;
                case BufferFormat.ABGR:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_ABGR;
                case BufferFormat.ABGR10:
                    return NV_BufferFormat.NV_ENC_BUFFER_FORMAT_ABGR10;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bufferFormat), bufferFormat, null);
            }
        }
    }
}
