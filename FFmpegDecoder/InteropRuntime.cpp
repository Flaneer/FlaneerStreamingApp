#include "DecodingRuntime.h"
#include "InteropStructs.h"

#define EXPORT extern "C" __declspec(dllexport)

DecodingRuntime decodingRuntime;

EXPORT void Init(const VideoFrameSettings settings)
{
	decodingRuntime = DecodingRuntime();
	decodingRuntime.InitAVIOReader();
	decodingRuntime.InitVSD();
	decodingRuntime.InitVSC(settings);
}

EXPORT bool FulfilFrameRequest(FrameRequest& frame_request)
{
	return decodingRuntime.FulfilFrameRequest(frame_request);
}

EXPORT void Cleanup()
{
	decodingRuntime.~DecodingRuntime();
}