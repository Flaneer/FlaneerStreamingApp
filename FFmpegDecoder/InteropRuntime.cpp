#include "DecodingRuntime.h"
#include "InteropStructs.h"

#define EXPORT extern "C" __declspec(dllexport)

DecodingRuntime decodingRuntime;

EXPORT void Init(const VideoFrameSettings settings, FrameRequest& firstFrame)
{
	decodingRuntime = DecodingRuntime();
	decodingRuntime.InitAVIOReader(firstFrame);
	decodingRuntime.InitVSD();
	decodingRuntime.InitVSC(settings);
	decodingRuntime.FulfilFrameRequest(firstFrame);
}

EXPORT bool FulfilFrameRequest(FrameRequest& frame_request)
{
	return decodingRuntime.FulfilFrameRequest(frame_request);
}

EXPORT void Cleanup()
{
	decodingRuntime.~DecodingRuntime();
}
