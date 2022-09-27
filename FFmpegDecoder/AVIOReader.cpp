#include "AVIOReader.h"

#include <fstream>
#include <iostream>

static int read(void* opaque, unsigned char* buf, int buf_size)
{
    auto bd = static_cast<struct buffer_data*>(opaque);

    if(buf_size > bd->size)
        std::cout << "Buf size: " << buf_size << " Read Size: " << bd->size << "\n";

	buf_size = FFMIN(bd->size, buf_size);

    memcpy(buf, bd->ptr, buf_size);
    bd->ptr += buf_size;
    bd->size -= buf_size;

    if(buf_size == 0)
        throw;

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

    bd = buffer;
    
    AllocAvioContext();
}

void AVIOReader::Cleanup() const
{
	av_free(AvioCtx);
    av_free(bufferPtr);
}

void AVIOReader::AllocAvioContext()
{
    AvioCtx = avio_alloc_context(bufferPtr, bufferSize, 0, &bd, &read, NULL, &seek);
}
