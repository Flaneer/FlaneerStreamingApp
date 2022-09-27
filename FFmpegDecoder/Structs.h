#include <cstdint>
#include <string>
#pragma once

extern "C"
{
#include "libavdevice/avdevice.h"
#include "libavutil/channel_layout.h"
#include "libavutil/mathematics.h"
#include "libavutil/opt.h"
#include "libavutil/imgutils.h"
#include "libavformat/avformat.h"
#include "libswscale/swscale.h"
#include <libavcodec/avcodec.h>
#include <libavformat/avio.h>
}

struct buffer_data {
    uint8_t* ptr;
    size_t size; ///< size left in the buffer
}; 

typedef struct
{
    int height;
    int width;
}Size;

static std::string AVErr(int error)
{
    char buffer[128];
    av_strerror(error, buffer, 128);
    return buffer;
}
