#pragma once

#include "DDAImpl.h"
#include "InteropStructs.h"
#include "NvEncoderD3D11.h"


class CaptureRuntime
{
    /// DDA wrapper object, defined in DDAImpl.h
    DDAImpl* pDDAWrapper = nullptr;
    /// NVENCODE API wrapper. Defined in NvEncoderD3D11.h. This class is imported from NVIDIA Video SDK
    NvEncoderD3D11* pEnc = nullptr;
    /// D3D11 device context used for the operations demonstrated in this application
    ID3D11Device* pD3DDev = nullptr;
    /// D3D11 device context
    ID3D11DeviceContext* pCtx = nullptr;
    /// D3D11 RGB Texture2D object that recieves the captured image from DDA
    ID3D11Texture2D* pDupTex2D = nullptr;
    /// D3D11 YUV420 Texture2D object that sends the image to NVENC for video encoding
    ID3D11Texture2D* pEncBuf = nullptr;
    /// NVENCODEAPI session intialization parameters
    NV_ENC_INITIALIZE_PARAMS encInitParams = { 0 };
    /// NVENCODEAPI video encoding configuration parameters
    NV_ENC_CONFIG encConfig = { 0 };

    
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

public:
    CaptureRuntime(VideoCaptureSettings capture_settings, H264CodecSettings cocdec_settings);
    ~CaptureRuntime();

    HRESULT Init();
};

