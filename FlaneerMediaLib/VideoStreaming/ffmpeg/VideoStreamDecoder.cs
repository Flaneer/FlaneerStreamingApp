using System.Drawing;
using System.Text;
using FFmpeg.AutoGen;
using FlaneerMediaLib.Logging;

using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Decodes video frames
    /// </summary>
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private AVFrame* framePtr;
        private AVFrame* receivedFrame;
        private AVPacket* packetPtr;
        private AVFormatContext* avfCtxPtr;
        
        internal AVCodecContext* CodecContextPtr;

        private int frameCount;
        private bool usingHardwareDecoding => CodecContextPtr->hw_device_ctx != null;
        
        /// <summary>
        /// The height and width of the video frame
        /// </summary>
        public Size SourceSize;
        /// <summary>
        /// The pixel format of the source image for decoding
        /// </summary>
        public AVPixelFormat SourcePixelFormat;

        private readonly Logger logger;

        /// <summary>
        /// ctor
        /// </summary>
        public VideoStreamDecoder()
        {
            logger = Logger.GetLogger(this);
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        public VideoStreamDecoder(AVIOContext* avioCtx)
        {
            logger = Logger.GetLogger(this);

            ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
            var frameSettings = clArgStore.GetParams(CommandLineArgs.FrameSettings);
            var width =  Int16.Parse(frameSettings[0]);
            var height = Int16.Parse(frameSettings[1]);
            
            Init(avioCtx, width, height);
        }

        /// <summary>
        /// Initializes the decoder
        /// </summary>
        internal void Init(AVIOContext* avioCtx, int width, int height)
        {
            //Allocate an AVFormatContext. avformat_free_context() can be used to free the context and everything allocated by the framework within it.
            var ctx = FF.avformat_alloc_context();
            ctx->pb = avioCtx;

            var arbitrarytext = Encoding.UTF8.GetString(Encoding.Default.GetBytes(""));
            var inFmt = FF.av_find_input_format("h264");
            FF.avformat_open_input(&ctx, arbitrarytext, inFmt, null);

            AVCodec* codec = FF.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264);
            //Allocate an AVCodecContext and set its fields to default values. The resulting struct should be freed with avcodec_free_context().
            CodecContextPtr = FF.avcodec_alloc_context3(codec);
            
            CodecContextPtr->width = width;
            CodecContextPtr->height = height;
            CodecContextPtr->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;

            var hwDec = HwDecodeHelper.GetHWDecoder();
            if (hwDec != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                FF.av_hwdevice_ctx_create(&CodecContextPtr->hw_device_ctx, hwDec, null, null, 0);
            }

            FF.avcodec_open2(CodecContextPtr, codec, null);

            packetPtr = FF.av_packet_alloc();
            framePtr = FF.av_frame_alloc();
            if (usingHardwareDecoding)
                receivedFrame = FF.av_frame_alloc();

            SourceSize = new Size(CodecContextPtr->width, CodecContextPtr->height);
            SourcePixelFormat = usingHardwareDecoding ? HwDecodeHelper.GetHWPixelFormat(hwDec) : CodecContextPtr->pix_fmt;

            avfCtxPtr = ctx;
        }

        /// <summary>
        /// Decode the next frame in the sequence
        /// </summary>
        public AVFrame DecodeNextFrame()
        {
            FF.av_frame_unref(framePtr);
            if(usingHardwareDecoding)
                FF.av_frame_unref(receivedFrame);
            
            int error;
            frameCount++;
            logger.Trace($"Decoding frame {frameCount}");
            do
            {
                try
                {
                    do
                    {
                        FF.av_packet_unref(packetPtr);
                        error = FF.av_read_frame(avfCtxPtr, packetPtr);

                        if (error == FF.AVERROR_EOF)
                        {
                            throw new Exception("error == ffmpeg.AVERROR_EOF");
                        }
                    } while (packetPtr->stream_index != 0);

                    FF.avcodec_send_packet(CodecContextPtr, packetPtr);
                }
                finally
                {
                    FF.av_packet_unref(packetPtr);
                }

                error = FF.avcodec_receive_frame(CodecContextPtr, framePtr);
            } while (error == FF.AVERROR(FF.EAGAIN));

            logger.Trace($"Frame {CodecContextPtr->frame_number}" +
                           $"(type={Convert.ToChar(FF.av_get_picture_type_char(framePtr->pict_type))}," +
                           $" size={framePtr->pkt_size} bytes, format={framePtr->format}) " +
                           $"pts {framePtr->pts} key_frame {framePtr->key_frame} (DTS {framePtr->coded_picture_number})");
            
            if (usingHardwareDecoding)
            {
                FF.av_hwframe_transfer_data(receivedFrame, framePtr, 0);
                return *receivedFrame;
            }

            return *framePtr;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var pFrame = framePtr;
            FF.av_frame_free(&pFrame);

            var pPacket = packetPtr;
            FF.av_packet_free(&pPacket);

            FF.avcodec_close(CodecContextPtr);
            var pFormatContext = avfCtxPtr;
            FF.avformat_close_input(&pFormatContext);
        }
    }
}



