using System.Drawing;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FlaneerMediaLib.Logging;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Converts decoded video frames to a given format
    /// </summary>
    public sealed unsafe class VideoFrameConverter : IDisposable
    {
        private readonly IntPtr convertedFrameBufferPtr;
        private readonly Size destinationSize;
        private readonly byte_ptrArray4 dstData;
        private readonly int_array4 dstLinesize;
        private readonly SwsContext* pConvertContext;

        private readonly Logger logger;
        
        /// <summary>
        /// ctor
        /// </summary>
        public VideoFrameConverter(Size sourceSize, AVPixelFormat sourcePixelFormat,
            Size destinationSize, AVPixelFormat destinationPixelFormat)
        {
            logger = Logger.GetLogger(this);
            this.destinationSize = destinationSize;

            pConvertContext = FF.sws_getContext(sourceSize.Width, sourceSize.Height, sourcePixelFormat,
                destinationSize.Width, destinationSize.Height, destinationPixelFormat,
                FF.SWS_FAST_BILINEAR, null, null, null);
            
            if (pConvertContext == null)
                throw new ApplicationException("Could not initialize the conversion context.");

            var convertedFrameBufferSize = FF.av_image_get_buffer_size(destinationPixelFormat,
                destinationSize.Width, destinationSize.Height, 1);
            
            convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            dstData = new byte_ptrArray4();
            dstLinesize = new int_array4();

            FF.av_image_fill_arrays(ref dstData,
                ref dstLinesize,
                (byte*) convertedFrameBufferPtr,
                destinationPixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                1);
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Marshal.FreeHGlobal(convertedFrameBufferPtr);
            FF.sws_freeContext(pConvertContext);
        }

        /// <summary>
        /// Convert the given frame
        /// </summary>
        public AVFrame Convert(AVFrame sourceFrame)
        {
            var sh = FF.sws_scale(pConvertContext,
                sourceFrame.data,
                sourceFrame.linesize,
                0,
                sourceFrame.height,
                dstData,
                dstLinesize);
            
            if(sh < 0)
                logger.Error($"Error converting packet: {FFmpegHelper.AVErr(sh)}");

            var data = new byte_ptrArray8();
            data.UpdateFrom(dstData);
            var linesize = new int_array8();
            linesize.UpdateFrom(dstLinesize);

            return new AVFrame
            {
                data = data,
                linesize = linesize,
                width = destinationSize.Width,
                height = destinationSize.Height
            };
        }
    }
}
