#pragma once
#include "nvEncodeAPI.h"

/// <summary>
/// Settings that specify the specs fo the video capture feed
/// </summary>
typedef struct
{
    short Width;
    short Height;

    short MaxFPS;
} VideoCaptureSettings;

/// <summary>
/// Provides information to configure 
/// </summary>
typedef struct
{
    NV_ENC_BUFFER_FORMAT Format;
    short GoPLength;
} H264CodecSettings;

/// <summary>
/// Given to the native lib with information to generate a frame and
/// an empty data pointer to be set to the address of the frame data
/// </summary>
typedef struct
{
    short Width;
    short Height;

    unsigned char* Data;
    INT32 Buffersize;
} FrameRequest;