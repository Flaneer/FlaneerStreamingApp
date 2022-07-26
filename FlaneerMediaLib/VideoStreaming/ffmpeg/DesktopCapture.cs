using FFmpeg.AutoGen;
using FlaneerMediaLib.VideoDataTypes;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg;

/// <summary>
/// 
/// </summary>
public unsafe class DesktopCapture : IVideoSource
{
    /// <inheritdoc />
    public ICodecSettings CodecSettings { get; private set; }

    /// <inheritdoc />
    public FrameSettings FrameSettings { get; private set; }
    
    private AVFormatContext *pFormatCtx;
    private AVCodecContext *pCodecCtx;
    private AVCodec *pCodec;
    
    private AVFrame* framePtr;
    private AVPacket* packetPtr;
    
    int i, videoindex;
    /// <summary>
    /// ctor
    /// </summary>
    public DesktopCapture()
    {
    }
    
    /// <inheritdoc />
    public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
    {
        FrameSettings = frameSettingsIn;
        CodecSettings = codecSettingsIn;
        
        var ctx = FF.avformat_alloc_context();
        
        //Use gdigrab
        AVDictionary* options;
        FF.av_dict_set(&options,"framerate",$"{frameSettingsIn.MaxFPS}",0);
        FF.av_dict_set(&options,"video_size",$"{frameSettingsIn.Width}x{frameSettingsIn.Height}",0);
        AVInputFormat *ifmt = FF.av_find_input_format("gdigrab");
        FF.avformat_open_input(&ctx, "desktop", ifmt, &options);

        FF.avformat_find_stream_info(pFormatCtx, null);
        
        videoindex=-1;
        for(i=0; i<pFormatCtx->nb_streams; i++)
        {
            if(pFormatCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                videoindex=i;
                break;
            }
        }

        if(videoindex==-1)
        {
            return false;
        }
        
        pCodec = FF.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
        
        pCodecCtx = FF.avcodec_alloc_context3(pCodec);
        
        pCodec=FF.avcodec_find_decoder(pCodecCtx->codec_id);
        
        FF.avcodec_open2(pCodecCtx, pCodec, null);
        
        framePtr = FF.av_frame_alloc();
        packetPtr = FF.av_packet_alloc();
        
        pFormatCtx = ctx;
        
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var pFrame = framePtr;
        FF.av_frame_free(&pFrame);

        var pPacket = packetPtr;
        FF.av_packet_free(&pPacket);

        FF.avcodec_close(pCodecCtx);
        var pFormatContext = pFormatCtx;
        FF.avformat_close_input(&pFormatContext);
    }
    
    /// <inheritdoc />
    public bool GetFrame(out IVideoFrame frame)
    {
        var avFrame = EncodeDesktopFrame();
        var convertedFrameSize = avFrame.height * avFrame.linesize[0];
        if (avFrame.height == FrameSettings.Height)
        {
            frame = new UnmanagedVideoFrame()
            {
                Codec = VideoCodec.H264,
                Height = (short)FrameSettings.Height,
                Width = (short)FrameSettings.Width,
                FrameData = (IntPtr)avFrame.data[0],
                FrameSize = convertedFrameSize
                
            };
            return true;
        }
        else
        {
            frame = new UnmanagedVideoFrame();
            return false;
        }
    }
    
    /// <summary>
    /// Capture and encode a frame from desktop capture
    /// </summary>
    private AVFrame EncodeDesktopFrame()
    {
        FF.av_frame_unref(framePtr);

        int error;
        do
        {
            try
            {
                do
                {
                    FF.av_packet_unref(packetPtr);
                    error = FF.av_read_frame(pFormatCtx, packetPtr);

                    if (error == FF.AVERROR_EOF)
                    {
                        throw new Exception("error == ffmpeg.AVERROR_EOF");
                    }
                } while (packetPtr->stream_index != 0);

                FF.avcodec_send_packet(pCodecCtx, packetPtr);
            }
            finally
            {
                FF.av_packet_unref(packetPtr);
            }

            error = FF.avcodec_receive_frame(pCodecCtx, framePtr);
        } while (error == FF.AVERROR(FF.EAGAIN));

        return *framePtr;
    }
}
