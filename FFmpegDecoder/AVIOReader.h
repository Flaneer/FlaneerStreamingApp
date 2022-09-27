#pragma once
#include "Structs.h"

class AVIOReader
{
public:
    /// <summary>
    /// AVIO context pointer, used for ffmpeg
    /// </summary>
    AVIOContext* AvioCtx;

private:
    int bufferSize;
    unsigned char* bufferPtr;

    buffer_data bd{ 0 };

public:
    AVIOReader() = default;

    void SetBuffer(buffer_data buffer);

    void Cleanup() const;

private:
    void AllocAvioContext();
};
