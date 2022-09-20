//#include <iostream>
//#include <filesystem>
//#include <fstream>
//
//#include "AVIOReader.h"
//#include "Structs.h"
//#include "VideoStreamConverter.h"
//#include "VideoStreamDecoder.h"
//
//
//static void save_gray_frame(unsigned char* buf, int wrap, int xsize, int ysize, char* filename)
//{
//    FILE* f;
//    int i;
//    f = fopen(filename, "w");
//    // writing the minimal required header for a pgm file format
//    // portable graymap format -> https://en.wikipedia.org/wiki/Netpbm_format#PGM_example
//    fprintf(f, "P5\n%d %d\n%d\n", xsize, ysize, 255);
//
//    // writing line by line
//    for (i = 0; i < ysize; i++)
//        fwrite(buf + i * wrap, 1, xsize, f);
//    fclose(f);
//}
//
//buffer_data RefreshInputStream(int videoIterator)
//{
//    char path[128];
//    sprintf_s(path, "C:\\Users\\Tom\\Videos\\VideoAsSequence\\SampleStreamh264Sequence\\%d.h264", videoIterator);
//    
//	std::filesystem::path p{ path };
//    const int fiLength = file_size(p);
//    std::ifstream infile(p);
//    char* buffer = new char[fiLength];
//    infile.read(buffer, fiLength);
//
//    buffer_data bd{ 0 };
//    bd.ptr = reinterpret_cast<uint8_t*>(buffer);
//    bd.size = fiLength;
//
//    return bd;
//}
//
//int main()
//{
//    int videoIterator = 1;
//    auto fileBuffer = RefreshInputStream(videoIterator);
//
//    avdevice_register_all();
//
//    AVIOReader avio_reader = AVIOReader();
//
//    VideoStreamDecoder vsd = VideoStreamDecoder(avio_reader.AvioCtx);
//
//    auto destinationSize = vsd.SourceSize;
//    auto destinationPixelFormat = AV_PIX_FMT_RGB24;
//    auto vsc = VideoStreamConverter(vsd.SourceSize, vsd.SourcePixelFormat, destinationSize, destinationPixelFormat);
//
//    for (; videoIterator < 10; ++videoIterator)
//    {
//        fileBuffer = RefreshInputStream(videoIterator);
//        std::cout << "Buf:";
//        for (int i = 0; i < 30; ++i)
//        {
//            std::cout << *(fileBuffer.ptr + i);
//        }
//        std::cout << "\n";
//        avio_reader.SetBuffer(fileBuffer);
//        auto frame = vsc.Convert(vsd.TryDecodeNextFrame());
//
//        char frame_filename[1024];
//        snprintf(frame_filename, sizeof(frame_filename), "%s-%d.pgm", "frame", videoIterator);
//        save_gray_frame(frame.data[0], frame.linesize[0], frame.width, frame.height, frame_filename);
//    }
//}