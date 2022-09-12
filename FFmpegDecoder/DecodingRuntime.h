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

	int it = 0;

public:
	DecodingRuntime() = default;

	void InitAVIOReader(FrameRequest& firstFrame);
	void InitVSD();
	void InitVSC(VideoFrameSettings settings);

	bool FulfilFrameRequest(FrameRequest& frame_request);

	void Cleanup();
};

