using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using FFmpeg.AutoGen;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg;

/// <summary>
/// 
/// </summary>
public unsafe class DesktopCapture : IVideoSource, IEncoder
{
    private int videoStreamIndx;
    private AVFormatContext* ifmtCtx;
    private AVCodecContext* avcodecContx;
    private AVFormatContext* ofmtCtx;
    private AVStream* videoStream;
    private AVCodecContext* avCntxOut;
    private AVPacket* avPkt;
    private AVFrame* avFrame;
    private AVFrame* outFrame;
    private SwsContext* swsCtx;
    private int encPacketCounter;

    private FileStream fs = new FileStream("testOut.h264", FileMode.Create);
    private readonly Logger logger;
    private VideoFrameConverter vfc;

    /// <inheritdoc />
    public ICodecSettings CodecSettings { get; private set; }

    /// <inheritdoc />
    public FrameSettings FrameSettings { get; private set; }

    /// <summary>
    /// ctor
    /// </summary>
    public DesktopCapture()
    {
        logger = Logger.GetLogger(this);

        FF.avdevice_register_all();
    }

    /// <inheritdoc />
    public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
    {
        CodecSettings = codecSettingsIn;
        //TODO: if is h264 etc...
        FrameSettings = frameSettingsIn;
        
        AVCodecParameters* avCodecParOut = ConfigureAvCodec();

        AVDictionary* options = ConfigureScreenCapture();

        AVInputFormat* ifmt = FF.av_find_input_format("gdigrab");
        var ifmtCtxLocal = FF.avformat_alloc_context();
        if (FF.avformat_open_input(&ifmtCtxLocal, "desktop", ifmt, &options) < 0)
        {
            Console.Error.WriteLine("Error in opening file");
            return false;
        }
        ifmtCtx = ifmtCtxLocal;
        
        videoStreamIndx = GetVideoStreamIndex();

        AVCodecParameters* avCodecParIn = FF.avcodec_parameters_alloc();
        avCodecParIn = ifmtCtx->streams[videoStreamIndx]->codecpar;
        
        AVCodec* avCodec = FF.avcodec_find_decoder(avCodecParIn->codec_id);
        if (avCodec == null)
        {
            Console.Error.WriteLine("unable to find the decoder");
            return false;
        }

        avcodecContx = FF.avcodec_alloc_context3(avCodec);
        if (FF.avcodec_parameters_to_context(avcodecContx, avCodecParIn) < 0)
        {
            Console.Error.WriteLine("error in converting the codec contexts");
            return false;
        }

        //av_dict_set
        int value = FF.avcodec_open2(avcodecContx, avCodec, null); //Initialize the AVCodecContext to use the given AVCodec.
        if (value < 0)
        {
            Console.Error.WriteLine("unable to open the av codec");
            return false;
        }
        
        AVOutputFormat* ofmt = FF.av_guess_format("h264", null, null);
        
        if (ofmt == null)
        {
            Console.Error.WriteLine("error in guessing the video format. try with correct format");
            return false;
        }

        var ofmtCtxLocal = FF.avformat_alloc_context();
        FF.avformat_alloc_output_context2(&ofmtCtxLocal, ofmt, null, null);
        if (ofmtCtxLocal == null)
        {
            Console.Error.WriteLine("error in allocating av format output context");
            return false;
        }
        ofmtCtx = ofmtCtxLocal;

        AVCodec* avCodecOut = FF.avcodec_find_encoder(avCodecParOut->codec_id);
        if (avCodecOut == null)
        {
            Console.Error.WriteLine("unable to find the encoder");
            return false;
        }

        videoStream = FF.avformat_new_stream(ofmtCtx, avCodecOut);
        if (videoStream == null)
        {
            Console.Error.WriteLine("error in creating a av format new stream");
            return false;
        }

        avCntxOut = FF.avcodec_alloc_context3(avCodecOut);
        if (avCntxOut == null)
        {
            Console.Error.WriteLine("error in allocating the codec contexts");
            return false;
        }

        if (FF.avcodec_parameters_copy(videoStream->codecpar, avCodecParOut) < 0)
        {
            Console.Error.WriteLine("Codec parameter cannot copied");
            return false;
        }

        if (FF.avcodec_parameters_to_context(avCntxOut, avCodecParOut) < 0)
        {
            Console.Error.WriteLine("error in converting the codec contexts");
            return false;
        }

        avCntxOut->gop_size = 30; //3; //Use I-Frame frame every 30 frames.
        avCntxOut->max_b_frames = 0;
        avCntxOut->time_base.num = 1;
        avCntxOut->time_base.den = FrameSettings.MaxFPS;
        
        //ffmpeg.avio_open(&ofmtCtx->pb, "", ffmpeg.AVIO_FLAG_READ_WRITE);
        
        if (FF.avformat_write_header(ofmtCtx, null) < 0)
        {
            Console.Error.WriteLine("error in writing the header context");
            return false;
        }

        value = FF.avcodec_open2(avCntxOut, avCodecOut, null); //Initialize the AVCodecContext to use the given AVCodec.
        if (value < 0)
        {
            Console.Error.WriteLine("unable to open the av codec");
            return false;
        }

        if (avcodecContx->codec_id == AVCodecID.AV_CODEC_ID_H264)
        {
            FF.av_opt_set(avCntxOut->priv_data, "preset", "ultrafast", 0);
            FF.av_opt_set(avCntxOut->priv_data, "zerolatency", "1", 0);
            FF.av_opt_set(avCntxOut->priv_data, "tune", "ull", 0);
        }

        if ((ofmtCtx->oformat->flags & FF.AVFMT_GLOBALHEADER) != 0)
        {
            avCntxOut->flags |= FF.AV_CODEC_FLAG_GLOBAL_HEADER;
        }
        
        CreateFrames(avCodecParIn, avCodecParOut);

        swsCtx = FF.sws_alloc_context();
        if (FF.sws_init_context(swsCtx, null, null) < 0)
        {
            Console.Error.WriteLine("Unable to Initialize the swscaler context sws_context.");
            return false;
        }

        swsCtx = FF.sws_getContext(avcodecContx->width, avcodecContx->height, avcodecContx->pix_fmt,
            avCntxOut->width, avCntxOut->height, avCntxOut->pix_fmt, FF.SWS_FAST_BILINEAR,
            null, null, null);
        if (swsCtx == null)
        {
            Console.Error.WriteLine(" Cannot allocate SWC Context");
            return false;
        }

        
        var size = new Size(1920, 1080);
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
        vfc = new VideoFrameConverter(size, avcodecContx->pix_fmt, size, destinationPixelFormat);
        
        return true;
    }


    /// <inheritdoc />
    public bool GetFrame(out IVideoFrame frame)
    {
        frame = GetFrame();
        return frame != new UnmanagedVideoFrame();
    }

    /// <inheritdoc />
    public IVideoFrame GetFrame()
    {
        var frame = CaptureEncodedFrame();
        if (frame.Item1 == IntPtr.Zero)
            return new UnmanagedVideoFrame();

        return new UnsafeUnmanagedVideoFrame()
        {
            Codec = VideoCodec.H264, //TODO: Set this better
            Height = (short) FrameSettings.Height,
            Width = (short) FrameSettings.Width,
            FrameData = (byte*) frame.Item1,
            FrameSize = frame.Item2
        };

        /*return new UnmanagedVideoFrame()
        {
            Codec = VideoCodec.H264, //TODO: Set this better
            Height = (short) FrameSettings.Height,
            Width = (short) FrameSettings.Width,
            FrameData = frame.Item1,
            FrameSize = frame.Item2
        };*/
    }
    
    private Tuple<IntPtr, int> CaptureEncodedFrame()
    {
        Tuple<IntPtr, int> ret = new Tuple<IntPtr, int>(IntPtr.Zero, 0);
        
        avPkt = FF.av_packet_alloc();
        AVPacket* outPacket = FF.av_packet_alloc();
        
        FF.av_read_frame(ifmtCtx, avPkt);
        FF.avcodec_send_packet(avcodecContx, avPkt);
        FF.avcodec_receive_frame(avcodecContx, avFrame);
        var f = vfc.Convert(*avFrame);
        ret = new Tuple<IntPtr, int>((IntPtr)f.data[0], f.pkt_size);
        return ret;
        
        while (FF.av_read_frame(ifmtCtx, avPkt) >= 0)
        {
            if (avPkt->stream_index != videoStreamIndx) continue;
            
            FF.avcodec_send_packet(avcodecContx, avPkt);
            if (FF.avcodec_receive_frame(avcodecContx, avFrame) >= 0) // Frame successfully decoded :)
            {
                outPacket->data = null; // packet data will be allocated by the encoder
                outPacket->size = 0;

                outPacket->pts = FF.av_rescale_q(encPacketCounter, avCntxOut->time_base, videoStream->time_base);
                if (outPacket->dts != FF.AV_NOPTS_VALUE)
                    outPacket->dts = FF.av_rescale_q(encPacketCounter, avCntxOut->time_base, videoStream->time_base);
                
                outPacket->dts = FF.av_rescale_q(encPacketCounter, avCntxOut->time_base, videoStream->time_base);
                outPacket->duration = FF.av_rescale_q(1, avCntxOut->time_base, videoStream->time_base);
                
                outFrame->pts = FF.av_rescale_q(encPacketCounter, avCntxOut->time_base, videoStream->time_base);
                outFrame->pkt_duration = FF.av_rescale_q(encPacketCounter, avCntxOut->time_base, videoStream->time_base);
                encPacketCounter++;
                
                int sts = FF.sws_scale(swsCtx,
                    avFrame->data, avFrame->linesize,  0, avFrame->height,
                    outFrame->data, outFrame->linesize);

                if (sts < 0)
                    Console.Error.WriteLine("Error while executing sws_scale");

                /* make sure the frame data is writable */
                var err = FF.av_frame_make_writable(outFrame); // SOMETHING IS WRONG WITH OUT FRAME ?!?!!?
                if (err < 0)
                    break;
                ret = Encode(avCntxOut, outFrame, outPacket);
                if (ret.Item1 == IntPtr.Zero)
                {
                    continue;
                }
                
                logger.Trace($" size={ret.Item2} bytes)");
                
                FF.av_frame_unref(avFrame);
                FF.av_packet_unref(avPkt);
                return ret;
            }
        }

        return ret;
    }
    
    private Tuple<IntPtr, int> Encode(AVCodecContext *encCtx, AVFrame *frame, AVPacket *pkt)
    {
        FF.av_packet_unref(pkt);
        /* send the frame to the encoder */
        int err = FF.avcodec_send_frame(encCtx, frame);
        if (err < 0) 
        {
            Console.Error.WriteLine("error sending a frame for encoding");
            return new Tuple<IntPtr, int>(IntPtr.Zero, 0);
        }

        while (true) 
        {
            err = FF.avcodec_receive_packet(encCtx, pkt);
            if (err == FF.AVERROR(FF.EAGAIN) || err == FF.AVERROR_EOF)
                return new Tuple<IntPtr, int>(IntPtr.Zero, 0);
            if (err < 0)
            {
                Console.Error.WriteLine("error during encoding");
                return new Tuple<IntPtr, int>(IntPtr.Zero, 0);
            }

            logger.Debug($"encoded frame {pkt->pts} (size={pkt->size})"+
                         $"(type={Convert.ToChar(FF.av_get_picture_type_char(frame->pict_type))}," +
                         $" size={frame->pkt_size} bytes, format={frame->format}) " +
                         $"pts {frame->pts} key_frame {frame->key_frame} (DTS {frame->coded_picture_number})");
            return new Tuple<IntPtr, int>((IntPtr)pkt->data, pkt->size);
        }
    }
    
    [return: MaybeNull]
    private AVDictionary* ConfigureScreenCapture()
    {
        AVDictionary* options = null;
        //Try adding "-rtbufsize 100M" as in https://stackoverflow.com/questions/6766333/capture-windows-screen-with-ffmpeg
        FF.av_dict_set(&options, "rtbufsize", "100M", 0);
        FF.av_dict_set(&options, "framerate", FrameSettings.MaxFPS.ToString(), 0);
        FF.av_dict_set(&options, "video_size", FrameSettings.Width + "x" + FrameSettings.Height, 0);
        return options;
    }

    private AVCodecParameters* ConfigureAvCodec()
    {
        AVCodecParameters* avCodecParOut = FF.avcodec_parameters_alloc();
        avCodecParOut->width = FrameSettings.Width;
        avCodecParOut->height = FrameSettings.Height;
        avCodecParOut->bit_rate = 40000;
        avCodecParOut->codec_id = AVCodecID.AV_CODEC_ID_H264; //TODO: Set this better
        avCodecParOut->codec_type = AVMediaType.AVMEDIA_TYPE_VIDEO;
        avCodecParOut->format = 0;
        return avCodecParOut;
    }

    private int GetVideoStreamIndex()
    {
        int videoStreamIdx = -1;
        FF.avformat_find_stream_info(ifmtCtx, null);
        /* find the first video stream index . Also there is an API available to do the below operations */
        for (int i = 0; i < (int) ifmtCtx->nb_streams; i++) // find video stream position/index.
        {
            if (ifmtCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                videoStreamIdx = i;
                break;
            }
        }

        if (videoStreamIdx == -1)
        {
            Console.Error.WriteLine("unable to find the video stream index. (-1)");
        }

        return videoStreamIdx;
    }

    private void CreateFrames(AVCodecParameters* avCodecParIn, AVCodecParameters* avCodecParOut)
    {

        avFrame = FF.av_frame_alloc();
        avFrame->width = avcodecContx->width;
        avFrame->height = avcodecContx->height;
        avFrame->format = avCodecParIn->format;
        FF.av_frame_get_buffer(avFrame, 0);

        outFrame = FF.av_frame_alloc();
        outFrame->width = avCntxOut->width;
        outFrame->height = avCntxOut->height;
        outFrame->format = avCodecParOut->format;
        FF.av_frame_get_buffer(outFrame, 0);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var ifmtCtxLocal = ifmtCtx;
        FF.avformat_close_input(&ifmtCtxLocal);
        if (ifmtCtxLocal == null)
        {
            Console.Error.WriteLine("file closed successfully");
        }
        else
        {
            Console.Error.WriteLine("unable to close the file");
            return;
        }

        FF.avformat_free_context(ifmtCtxLocal);
        if (ifmtCtxLocal == null)
        {
            Console.Error.WriteLine("avformat free successfully");
        }
        else
        {
            Console.Error.WriteLine("unable to free avformat context");
            return;
        }

        //Free codec context.
        var avCntxOutLocal = avcodecContx;
        FF.avcodec_free_context(&avCntxOutLocal);

        if (avCntxOutLocal == null)
        {
            Console.Error.WriteLine("avcodec free successfully");
        }
        else
        {
            Console.Error.WriteLine("unable to free avcodec context");
        }
    }
}
