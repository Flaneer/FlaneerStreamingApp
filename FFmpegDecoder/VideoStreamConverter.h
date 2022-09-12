#pragma once

#include "Structs.h"

class VideoStreamConverter
{
private:
    uint8_t* convertedFrameBufferPtr;
    Size destinationSize{};
    uint8_t* dstData[4]{};
    int  dstLinesize[4]{};
    SwsContext* pConvertContext;
public:
    VideoStreamConverter() = default;
    VideoStreamConverter(Size sourceSize, AVPixelFormat sourcePixelFormat, Size destinationSize,
        AVPixelFormat destinationPixelFormat);
    void Cleanup() const;
    AVFrame Convert(AVFrame sourceFrame) const;
};

