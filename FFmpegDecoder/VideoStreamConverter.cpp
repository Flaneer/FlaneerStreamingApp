#include "VideoStreamConverter.h"

#include <iostream>

VideoStreamConverter::VideoStreamConverter(Size sourceSize, AVPixelFormat sourcePixelFormat, Size destinationSizeIn,
                                           AVPixelFormat destinationPixelFormat)
{
    destinationSize = destinationSizeIn;

    pConvertContext = sws_getContext(sourceSize.width,
        sourceSize.height,
        sourcePixelFormat,
        destinationSize.width,
        destinationSize.height,
        destinationPixelFormat,
        SWS_FAST_BILINEAR,
        nullptr,
        nullptr,
        nullptr);
    if (pConvertContext == nullptr)
        throw;

    auto convertedFrameBufferSize = av_image_get_buffer_size(destinationPixelFormat,
        destinationSize.width,
        destinationSize.height,
        1);

    convertedFrameBufferPtr = static_cast<uint8_t*>(av_malloc(
	    av_image_get_buffer_size(destinationPixelFormat, destinationSize.width, destinationSize.height, 1)));

    int error = av_image_fill_arrays(dstData,
        dstLinesize,
        convertedFrameBufferPtr,
        destinationPixelFormat,
        destinationSize.width,
        destinationSize.height,
        1);
    if (error < 0)
        std::cout << "av_image_fill_arrays" << AVErr(error) << "\n";
}

VideoStreamConverter::~VideoStreamConverter()
{
    sws_freeContext(pConvertContext);
}

AVFrame VideoStreamConverter::Convert(AVFrame sourceFrame)
{
    int height = sws_scale(pConvertContext,
        sourceFrame.data,
        sourceFrame.linesize,
        0,
        sourceFrame.height,
        dstData,
        dstLinesize);

    AVFrame avFrame = AVFrame();
    avFrame.data[0] = dstData[0];
    avFrame.linesize[0] = dstLinesize[0];
    avFrame.height = sourceFrame.height;
    avFrame.width = sourceFrame.width;

    return avFrame;
}
