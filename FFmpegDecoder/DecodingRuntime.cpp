#include "DecodingRuntime.h"

DecodingRuntime::DecodingRuntime(VideoFrameSettings settings)
{
	avioReader = AVIOReader();
	vsd = VideoStreamDecoder(avioReader.AvioCtx);
	Size size = { settings.Height, settings.Width };
	vsc = VideoStreamConverter(size, settings.PixelFormat,
		size, settings.PixelFormat);
}

bool DecodingRuntime::FulfilFrameRequest(FrameRequest& frame_request)
{
	buffer_data buffer = { frame_request.DataIn, static_cast<size_t>(frame_request.BufferSizeIn) };
	avioReader.SetBuffer(buffer);
	auto frame = vsc.Convert(vsd.TryDecodeNextFrame());

	if(frame.data[0] == nullptr)
		return false;

	frame_request.DataOut = frame.data[0];
	frame_request.Linesize = frame.linesize[0];
	frame_request.BufferSizeOut = frame.linesize[0] * frame.height;

	return true;
}

DecodingRuntime::~DecodingRuntime()
{
	avioReader.~AVIOReader();
	vsd.~VideoStreamDecoder();
	vsc.~VideoStreamConverter();
}
