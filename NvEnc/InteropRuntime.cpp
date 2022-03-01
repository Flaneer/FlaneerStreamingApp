#include "CaptureRuntime.h"

#define EXPORT extern "C" __declspec(dllexport)

CaptureRuntime capture_runtime;

EXPORT void Init(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings)
{
	capture_runtime = CaptureRuntime(capture_settings, codec_settings);
	capture_runtime.Init();
}

EXPORT HRESULT FulfilFrameRequest(FrameRequest& frame_request)
{
	return capture_runtime.FulfilFrameRequest(frame_request);
}

EXPORT void Cleanup()
{
	capture_runtime.Cleanup();
}