#pragma once
#include "AVIOReader.h"
#include "InteropStructs.h"
#include "VideoStreamConverter.h"
#include "VideoStreamDecoder.h"

class DecodingRuntime
{
private:
	AVIOReader avioReader;
	VideoStreamDecoder vsd;
	VideoStreamConverter vsc;

public:
	DecodingRuntime() = default;

	void InitAVIOReader();
	void InitVSD();
	void InitVSC(VideoFrameSettings settings);

	bool FulfilFrameRequest(FrameRequest& frame_request);

	void Cleanup();
};

