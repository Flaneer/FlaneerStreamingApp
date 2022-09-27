#include "VideoStreamDecoder.h"

#include <iostream>

#define FFMPEGCHECKERR(er, name)({if((er) != 0) {std::cout << (name) << AVErr(er) << "\n"; }})

static AVHWDeviceType GetHWDecoder()
{
    return AV_HWDEVICE_TYPE_NONE;

    AVHWDeviceType HWtype;
    std::vector<AVHWDeviceType> availableHWDecoders;

    auto type = AV_HWDEVICE_TYPE_NONE;
    int it = 0;
    int DXVA2 = 0;

    while ((type = av_hwdevice_iterate_types(type)) != AV_HWDEVICE_TYPE_NONE)
    {
        availableHWDecoders.push_back(type);
        if (type == AV_HWDEVICE_TYPE_DXVA2)
            DXVA2 = it;

        it++;
    }

    if (availableHWDecoders.size() == 0)
        return AV_HWDEVICE_TYPE_NONE;

    //Prefer AV_HWDEVICE_TYPE_DXVA2
    int decoderNumber = DXVA2;
    //else use the first suggested HW decoder
    if (decoderNumber == 0)
        decoderNumber = availableHWDecoders[0];

    return availableHWDecoders.size() < decoderNumber + 1 ? AV_HWDEVICE_TYPE_NONE : availableHWDecoders[decoderNumber];
}

static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
{
    switch (hWDevice)
    {
    case AV_HWDEVICE_TYPE_NONE: return  AV_PIX_FMT_NONE;
    case AV_HWDEVICE_TYPE_VDPAU: return  AV_PIX_FMT_VDPAU;
    case AV_HWDEVICE_TYPE_CUDA: return  AV_PIX_FMT_CUDA;
    case AV_HWDEVICE_TYPE_VAAPI: return  AV_PIX_FMT_VAAPI;
    case AV_HWDEVICE_TYPE_DXVA2: return  AV_PIX_FMT_NV12;
    case AV_HWDEVICE_TYPE_QSV: return  AV_PIX_FMT_QSV;
    case AV_HWDEVICE_TYPE_VIDEOTOOLBOX: return  AV_PIX_FMT_VIDEOTOOLBOX;
    case AV_HWDEVICE_TYPE_D3D11VA: return  AV_PIX_FMT_NV12;
    case AV_HWDEVICE_TYPE_DRM: return  AV_PIX_FMT_DRM_PRIME;
    case AV_HWDEVICE_TYPE_OPENCL: return  AV_PIX_FMT_OPENCL;
    case AV_HWDEVICE_TYPE_MEDIACODEC: return  AV_PIX_FMT_MEDIACODEC;
    default: return AV_PIX_FMT_NONE;
    }
}

VideoStreamDecoder::VideoStreamDecoder(AVIOContext* avioCtx)
{
    //Allocate an AVFormatContext. avformat_free_context() can be used to free the context and everything allocated by the framework within it.
    avfCtxPtr = avformat_alloc_context();
    avfCtxPtr->pb = avioCtx;

    auto inFmt = av_find_input_format("h264");
    const char* arbitraryText = "";
    int error = avformat_open_input(&avfCtxPtr, arbitraryText, inFmt, nullptr);

    if(error != 0)
        std::cout << "avformat_open_input: " << AVErr(error) << "\n";

    AVCodec* codec = avcodec_find_decoder(AV_CODEC_ID_H264);
    //Allocate an AVCodecContext and set its fields to default values. The resulting struct should be freed with avcodec_free_context().
    codecContextPtr = avcodec_alloc_context3(codec);
    //TODO: Set this from config
    codecContextPtr->width = 1920;
    codecContextPtr->height = 1080;
    codecContextPtr->pix_fmt = AV_PIX_FMT_YUV420P;

    auto hwDec = GetHWDecoder();
    if (hwDec != AV_HWDEVICE_TYPE_NONE)
    {
        error = av_hwdevice_ctx_create(&codecContextPtr->hw_device_ctx, hwDec, nullptr, nullptr, 0);
        if (error != 0)
            std::cout << "av_hwdevice_ctx_create" << AVErr(error) << "\n";
    }

    error = avcodec_open2(codecContextPtr, codec, nullptr);
    if (error != 0)
        std::cout << "avcodec_open2" << AVErr(error) << "\n";

    packetPtr = av_packet_alloc();
    framePtr = av_frame_alloc();
    if (usingHardwareDecoding())
        receivedFrame = av_frame_alloc();

    SourceSize = Size{ codecContextPtr->height, codecContextPtr->width};
    SourcePixelFormat = usingHardwareDecoding() ? GetHWPixelFormat(hwDec) : codecContextPtr->pix_fmt;
}

void VideoStreamDecoder::Cleanup()
{
    av_frame_free(&framePtr);    
    av_packet_free(&packetPtr);
    avcodec_close(codecContextPtr);
    avformat_close_input(&avfCtxPtr);
}

AVFrame VideoStreamDecoder::TryDecodeNextFrame()
{
    av_frame_unref(framePtr);
    if (usingHardwareDecoding())
        av_frame_unref(receivedFrame);

    int error;
    frameCount++;
    do
    {
        try
        {
            do
            {
                av_packet_unref(packetPtr);
                error = av_read_frame(avfCtxPtr, packetPtr);

                if (error == AVERROR_EOF)
                {
                    throw std::exception();
                }
                else if (error != 0)
                {
                    std::cout << AVErr(error) << "\n";
                }
            } while (packetPtr->stream_index != 0);

            avcodec_send_packet(codecContextPtr, packetPtr);
        }catch (...){}

        av_packet_unref(packetPtr);

        error = avcodec_receive_frame(codecContextPtr, framePtr);
    } while (error == AVERROR(EAGAIN));

    char path[128];
    sprintf_s(path, "Frame %d (type=%c, size=%d bytes, format=%d) pts %d key_frame %d [DTS %d]",
        codecContextPtr->frame_number,
        av_get_picture_type_char(framePtr->pict_type),
        framePtr->pkt_size,
        framePtr->format,
        framePtr->pts,
        framePtr->key_frame,
        framePtr->coded_picture_number);
    std::cout << path << "\n";

    if (usingHardwareDecoding())
    {
        av_hwframe_transfer_data(receivedFrame, framePtr, 0);
        return *receivedFrame;
    }

    return *framePtr;
}


