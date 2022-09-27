#pragma once
#include <libavutil/pixfmt.h>
#include <cstdint>

#include "Defs.h"

/// <summary>
/// Settings that specify the specs fo the video capture feed
/// </summary>
typedef struct
{
    short Width;
    short Height;

    Codec Codec;

    AVPixelFormat PixelFormat;
} VideoFrameSettings;

/// <summary>
/// Given to the native lib with information to generate a frame and
/// an empty data pointer to be set to the address of the frame data
/// </summary>
typedef struct
{
    short Width;
    short Height;

    short Linesize;

    unsigned char* DataIn;
    int32_t BufferSizeIn;

    unsigned char* DataOut;
    int32_t BufferSizeOut;
} FrameRequest;
