#include "DecodingRuntime.h"

#include <fstream>
#include <iostream>

static void save_gray_frame(unsigned char* buf, int size, int filenameSuffix)
{
	std::ofstream fs;
    char path[10];
    sprintf_s(path, "%d.h264", filenameSuffix);
	fs.open(path, std::ios::binary);
	fs.write((char*)buf, size);
	fs.close();
}

void DecodingRuntime::InitAVIOReader(FrameRequest& firstFrame)
{
	avioReader = AVIOReader();
	const buffer_data initBuff = { firstFrame.DataIn, static_cast<size_t>(firstFrame.BufferSizeIn)};
	avioReader.SetBuffer(initBuff);
}

void DecodingRuntime::InitVSD()
{
	vsd = VideoStreamDecoder(avioReader.AvioCtx);
}

void DecodingRuntime::InitVSC(VideoFrameSettings settings)
{
	Size size = { settings.Height, settings.Width };
	vsc = VideoStreamConverter(size, settings.PixelFormat,
	                           size, settings.PixelFormat);
}

static void save_gray_frame(unsigned char* buf, int wrap, int xsize, int ysize, int filename)
{
	FILE* f;
	int i;
	char txt[32];
	sprintf_s(txt, "%d.pgm", filename);
	fopen_s(&f, txt, "w");
	// writing the minimal required header for a pgm file format
	// portable graymap format -> https://en.wikipedia.org/wiki/Netpbm_format#PGM_example
	fprintf(f, "P5\n%d %d\n%d\n", xsize, ysize, 255);

	// writing line by line
	for (i = 0; i < ysize; i++)
		fwrite(buf + i * wrap, 1, xsize, f);
	fclose(f);
}

bool DecodingRuntime::FulfilFrameRequest(FrameRequest& frame_request)
{
	const buffer_data buffer = { frame_request.DataIn, static_cast<size_t>(frame_request.BufferSizeIn) };

	save_gray_frame(frame_request.DataIn, frame_request.BufferSizeIn, it++);

	avioReader.SetBuffer(buffer);
	auto frameOut = vsd.TryDecodeNextFrame();
	auto frame = vsc.Convert(frameOut);

	if(frame.data[0] == nullptr)
		return false;

	frame_request.DataOut = frame.data[0];
	frame_request.Linesize = frame.linesize[0];
	frame_request.BufferSizeOut = frame.linesize[0] * frame.height;

	save_gray_frame(frame_request.DataOut, frame_request.Linesize, frame_request.Width, frame_request.Height, 999);

	return true;
}

void DecodingRuntime::Cleanup()
{
	avioReader.Cleanup();
	vsd.Cleanup();
	vsc.Cleanup();
}
