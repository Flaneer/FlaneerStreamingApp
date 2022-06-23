#pragma once

#include "DDAImpl.h"
#include "InteropStructs.h"
#include "NvEncoderD3D11.h"
#include "RGBToNV12.h"


class CaptureRuntime
{
public:
    
    CaptureRuntime() = default;
    
    CaptureRuntime(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings);
    HRESULT FulfilFrameRequest(FrameRequest & frame_request);
    void Cleanup();
    ~CaptureRuntime();

    HRESULT Init();

private:
    /// <summary>
    /// DDA wrapper object, defined in DDAImpl.h
    /// </summary>
    DDAImpl* m_ddaWrapper = nullptr;
    /// <summary>
    /// NVENCODE API wrapper. Defined in NvEncoderD3D11.h. This class is imported from NVIDIA Video SDK
    /// </summary>
    NvEncoderD3D11* m_encoder = nullptr;
    /// <summary>
    /// D3D11 device context used for the operations demonstrated in this application
    /// </summary>
    ID3D11Device* m_d3dDevice = nullptr;
    /// <summary>
    /// D3D11 device context
    /// </summary>
    ID3D11DeviceContext* m_deviceContext = nullptr;
    /// <summary>
    /// D3D11 RGB Texture2D object that recieves the captured image from DDA
    /// </summary>
    ID3D11Texture2D* m_dupTex2D = nullptr;
    /// <summary>
    /// D3D11 YUV420 Texture2D object that sends the image to NVENC for video encoding
    /// </summary>
    ID3D11Texture2D* m_encBuf = nullptr;
    /// <summary>
    /// NVENCODEAPI session intialization parameters
    /// </summary>
    NV_ENC_INITIALIZE_PARAMS m_encInitParams = { 0 };
    /// <summary>
    /// NVENCODEAPI video encoding configuration parameters
    /// </summary>
    NV_ENC_CONFIG m_encConfig = { 0 };
    /// <summary>
    /// Preprocessingis required if captured images are of different size than encWidthxencHeight
    /// This application always uses this preprocessor
    /// </summary>
    RGBToNV12* m_colorConv = nullptr;

    std::vector<std::vector<uint8_t>> m_packet;

    int timeout() const
    {
        auto timeout = (1000.0f / m_maxFPS) * 2.0f;
        return (int)std::ceil(timeout);
    }

    short m_width;
    short m_height;
    short m_maxFPS;

    NV_ENC_BUFFER_FORMAT m_bufferFormat;
    short m_gopLength;
    short m_crf;
    GUID m_codecId;

    HRESULT InitDXGI();
	HRESULT InitDup();
	HRESULT InitEnc();
    HRESULT InitColorConv();
    HRESULT Capture();
    HRESULT Preproc();
    HRESULT Encode();
};

