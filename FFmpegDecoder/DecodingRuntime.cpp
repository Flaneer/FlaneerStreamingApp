#include "DecodingRuntime.h"

#include <fstream>
#include <iostream>

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
	                           size, AV_PIX_FMT_RGB24);
}

bool DecodingRuntime::FulfilFrameRequest(FrameRequest& frame_request)
{
	const buffer_data buffer = { frame_request.DataIn, static_cast<size_t>(frame_request.BufferSizeIn) };

	avioReader.SetBuffer(buffer);
	auto frameOut = vsd.TryDecodeNextFrame();
	frame = vsc.Convert(frameOut);

	if(frame.data[0] == nullptr)
		return false;

	frame_request.DataOut = frame.data[0];
	frame_request.Linesize = frame.linesize[0];
	frame_request.BufferSizeOut = frame.linesize[0] * frame.height;

	return true;
}

void DecodingRuntime::Cleanup()
{
	avioReader.Cleanup();
	vsd.Cleanup();
	vsc.Cleanup();
}
