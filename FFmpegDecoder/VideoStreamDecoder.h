#pragma once

#include <map>
#include <string>
#include <vector>

#include "Structs.h"

class VideoStreamDecoder
{
private:
    AVFrame* framePtr;
    AVFrame* receivedFrame;
    AVPacket* packetPtr;
    AVFormatContext* avfCtxPtr;
    AVCodecContext* codecContextPtr;
    int frameCount;

    bool usingHardwareDecoding() const { return codecContextPtr->hw_device_ctx != nullptr; }

public:
    Size SourceSize;
    AVPixelFormat SourcePixelFormat;

    VideoStreamDecoder() = default;
    VideoStreamDecoder(AVIOContext* avioCtx);
    void Cleanup();

    AVFrame TryDecodeNextFrame();
};
