using System.Drawing;
using System.Text;
using FFmpeg.AutoGen;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Decodes video frames
    /// </summary>
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private AVFrame* framePtr;
        private AVPacket* packetPtr;
        private AVFormatContext* avfCtxPtr;
        private readonly int streamIndex;
        private readonly AVCodecContext* CodecContextPtr;

        private int frameCount = 0;
        
        /// <summary>
        /// The height and width of the video frame
        /// </summary>
        public readonly Size SourceSize;
        /// <summary>
        /// The pixel format of the source image for decoding
        /// </summary>
        public readonly AVPixelFormat SourcePixelFormat;

        private Logger logger;

        /// <summary>
        /// ctor
        /// </summary>
        public VideoStreamDecoder(AVIOContext* avioCtx)
        {
            logger = Logger.GetLogger(this);
            
            var ctx = FFmpeg.AutoGen.ffmpeg.avformat_alloc_context();
            ctx->pb = avioCtx;
            
            var arbitrarytext = Encoding.UTF8.GetString(Encoding.Default.GetBytes(""));
            var inFmt = FFmpeg.AutoGen.ffmpeg.av_find_input_format("h264");
            FFmpeg.AutoGen.ffmpeg.avformat_open_input(&ctx, arbitrarytext, inFmt, null);
            //ffmpeg.avformat_find_stream_info(ctx, null);

            AVCodec* codec = FFmpeg.AutoGen.ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264);
            //streamIndex = ffmpeg.av_find_best_stream(ctx, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0);
            CodecContextPtr = FFmpeg.AutoGen.ffmpeg.avcodec_alloc_context3(codec);
            CodecContextPtr->width = 1920;
            CodecContextPtr->height = 1080;
            CodecContextPtr->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            
            //ffmpeg.avcodec_parameters_to_context(CodecContextPtr, ctx->streams[streamIndex]->codecpar);
            FFmpeg.AutoGen.ffmpeg.avcodec_open2(CodecContextPtr, codec, null);
            
            packetPtr = FFmpeg.AutoGen.ffmpeg.av_packet_alloc();
            framePtr = FFmpeg.AutoGen.ffmpeg.av_frame_alloc();

            SourceSize = new Size(CodecContextPtr->width, CodecContextPtr->height);
            SourcePixelFormat = CodecContextPtr->pix_fmt;
            
            avfCtxPtr = ctx;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var pFrame = framePtr;
            FFmpeg.AutoGen.ffmpeg.av_frame_free(&pFrame);

            var pPacket = packetPtr;
            FFmpeg.AutoGen.ffmpeg.av_packet_free(&pPacket);

            FFmpeg.AutoGen.ffmpeg.avcodec_close(CodecContextPtr);
            var pFormatContext = avfCtxPtr;
            FFmpeg.AutoGen.ffmpeg.avformat_close_input(&pFormatContext);
        }
        
        /// <summary>
        /// Decode the next frame in the sequence
        /// </summary>
        public AVFrame DecodeNextFrame()
        {
            FFmpeg.AutoGen.ffmpeg.av_frame_unref(framePtr);
            int error;
            frameCount++;
            logger.Trace($"Decoding frame {frameCount}");
            do
            {
                try
                {
                    do
                    {
                        FFmpeg.AutoGen.ffmpeg.av_packet_unref(packetPtr);
                        error = FFmpeg.AutoGen.ffmpeg.av_read_frame(avfCtxPtr, packetPtr);

                        if (error == FFmpeg.AutoGen.ffmpeg.AVERROR_EOF)
                        {
                            throw new Exception("error == ffmpeg.AVERROR_EOF");
                        }
                    } while (packetPtr->stream_index != streamIndex);

                    FFmpeg.AutoGen.ffmpeg.avcodec_send_packet(CodecContextPtr, packetPtr);
                }
                finally
                {
                    FFmpeg.AutoGen.ffmpeg.av_packet_unref(packetPtr);
                }

                error = FFmpeg.AutoGen.ffmpeg.avcodec_receive_frame(CodecContextPtr, framePtr);
            } while (error == FFmpeg.AutoGen.ffmpeg.AVERROR(FFmpeg.AutoGen.ffmpeg.EAGAIN));

            return *framePtr;
        }

    }
}



