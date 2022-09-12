#include "AVIOReader.h"

#include <iostream>


static int read(void* opaque, unsigned char* buf, int buf_size)
{
    auto bd = static_cast<struct buffer_data*>(opaque);
    buf_size = FFMIN(buf_size, bd->size);

    std::cout << "AVIO In:";
    for (int i = 0; i < 30; ++i)
    {
        std::cout << *(bd->ptr + i);
    }
    std::cout << "\n";

    memcpy(buf, bd->ptr, buf_size);
    bd->ptr += buf_size;
    bd->size -= buf_size;

    std::cout << "AVIO Buf:";
    for (int i = 0; i < 30; ++i)
    {
        std::cout << *(buf + i);
    }
    std::cout << "\n";

    return buf_size;
}

static int64_t seek(void* opaque, int64_t offset, int whence)
{
    auto bd = static_cast<struct buffer_data*>(opaque);
    return bd->size;
}

void AVIOReader::SetBuffer(buffer_data buffer)
{
    bufferSize = buffer.size;
    bufferPtr = static_cast<unsigned char*>(av_malloc(bufferSize));
    memcpy(bufferPtr, buffer.ptr, bufferSize);
    std::cout << "New Buffer: ";
    for (int i = 0; i < 30; ++i)
    {
        std::cout << *(bufferPtr + i);
    }
    std::cout << "\n";
    AllocAvioContext();
}

void AVIOReader::Cleanup() const
{
	av_free(AvioCtx);
    av_free(bufferPtr);
}

void AVIOReader::AllocAvioContext()
{
	
    bd.ptr = bufferPtr;
    bd.size = bufferSize;

    AvioCtx = avio_alloc_context(bufferPtr, bufferSize, 0, &bd, &read, NULL, &seek);
}
