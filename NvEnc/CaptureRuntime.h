#pragma once

#include "DDAImpl.h"
#include "InteropStructs.h"
#include "NvEncoderD3D11.h"


class CaptureRuntime
{
    /// DDA wrapper object, defined in DDAImpl.h
    DDAImpl* ddaWrapper = nullptr;
    /// NVENCODE API wrapper. Defined in NvEncoderD3D11.h. This class is imported from NVIDIA Video SDK
    NvEncoderD3D11* encoder = nullptr;
    /// D3D11 device context used for the operations demonstrated in this application
    ID3D11Device* D3DDevice = nullptr;
    /// D3D11 device context
    ID3D11DeviceContext* deviceContext = nullptr;
    /// D3D11 RGB Texture2D object that recieves the captured image from DDA
    ID3D11Texture2D* dupTex2D = nullptr;
    /// D3D11 YUV420 Texture2D object that sends the image to NVENC for video encoding
    ID3D11Texture2D* encBuf = nullptr;
    /// NVENCODEAPI session intialization parameters
    NV_ENC_INITIALIZE_PARAMS encInitParams = { 0 };
    /// NVENCODEAPI video encoding configuration parameters
    NV_ENC_CONFIG encConfig = { 0 };

    std::vector<std::vector<uint8_t>> vPacket;

    int timeout() const
    {
        //Come up with something more robust here
        return (1000.0f / MaxFPS) * 2.0f;
    }

    short Width;
    short Height;
    short MaxFPS;

    NV_ENC_BUFFER_FORMAT BufferFormat;
    short GoPLength;
    short CRF;
    GUID Codec_Id;

    HRESULT InitDXGI();
	HRESULT InitDup();
	HRESULT InitEnc();
    HRESULT Capture();
    HRESULT Preproc();
    HRESULT Encode();

public:
    CaptureRuntime() = default;
    CaptureRuntime(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings);
    HRESULT FulfilFrameRequest(FrameRequest& frame_request);
    void Cleanup();
    ~CaptureRuntime();

    HRESULT Init();
};

