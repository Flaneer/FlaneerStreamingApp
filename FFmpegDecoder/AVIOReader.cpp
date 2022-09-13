#include "AVIOReader.h"

#include <fstream>
#include <iostream>

static void save_gray_frame(unsigned char* buf, int size, int filenameSuffix)
{
    std::ofstream fs;
    char path[16];
    sprintf_s(path, "%d.h264", filenameSuffix);
    fs.open(path, std::ios::binary);
    fs.write((char*)buf, size);
    fs.close();
}

static int read(void* opaque, unsigned char* buf, int buf_size)
{
    auto bd = static_cast<struct buffer_data*>(opaque);

    std::cout << "Buf size: " << buf_size << " Read Size: " << bd->size << " Buf Size Bigger: " << (buf_size > bd->size) << "\n";

	buf_size = FFMIN(bd->size, buf_size);

    //save_gray_frame(bd->ptr, bd->size, 108);

    memcpy(buf, bd->ptr, buf_size);
    bd->ptr += buf_size;
    bd->size -= buf_size;

    //save_gray_frame(buf, buf_size, 104);

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

    //memcpy(bufferPtr, buffer.ptr, bufferSize);
    AllocAvioContext();
}

void AVIOReader::Cleanup() const
{
	av_free(AvioCtx);
    av_free(bufferPtr);
}

void AVIOReader::AllocAvioContext()
{
    /*bd.ptr = bufferPtr;
    bd.size = bufferSize;*/

    AvioCtx = avio_alloc_context(bufferPtr, bufferSize, 0, &bd, &read, NULL, &seek);
}
