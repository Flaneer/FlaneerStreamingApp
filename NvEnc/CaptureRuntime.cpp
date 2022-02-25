#include "CaptureRuntime.h"

#include "DDAImpl.h"
#include "Defs.h"
#include "nvEncodeAPI.h"
#include "NvEncoderD3D11.h"


/// Initialize DXGI pipeline
HRESULT CaptureRuntime::InitDXGI()
{
    HRESULT hr = S_OK;
    /// Driver types supported
    D3D_DRIVER_TYPE DriverTypes[] =
    {
        D3D_DRIVER_TYPE_HARDWARE,
        D3D_DRIVER_TYPE_WARP,
        D3D_DRIVER_TYPE_REFERENCE,
    };
    UINT NumDriverTypes = ARRAYSIZE(DriverTypes);

    /// Feature levels supported
    D3D_FEATURE_LEVEL FeatureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
        D3D_FEATURE_LEVEL_9_1
    };
    UINT NumFeatureLevels = ARRAYSIZE(FeatureLevels);
    D3D_FEATURE_LEVEL FeatureLevel = D3D_FEATURE_LEVEL_11_0;

    /// Create device
    for (UINT DriverTypeIndex = 0; DriverTypeIndex < NumDriverTypes; ++DriverTypeIndex)
    {
        hr = D3D11CreateDevice(nullptr, DriverTypes[DriverTypeIndex], nullptr, /*D3D11_CREATE_DEVICE_DEBUG*/0, FeatureLevels, NumFeatureLevels,
            D3D11_SDK_VERSION, &pD3DDev, &FeatureLevel, &pCtx);
        if (SUCCEEDED(hr))
        {
            // Device creation succeeded, no need to loop anymore
            break;
        }
    }
    return hr;
}

/// Initialize DDA handler
HRESULT CaptureRuntime::InitDup()
{
    HRESULT hr = S_OK;
    if (!pDDAWrapper)
    {
        pDDAWrapper = new DDAImpl(pD3DDev, pCtx);
        hr = pDDAWrapper->Init();
        returnIfError(hr);
    }
    return hr;
}

/// Initialize NVENCODEAPI wrapper
HRESULT CaptureRuntime::InitEnc()
{
    if (!pEnc)
    {
        //std::cout << "Video details: Width:" << w << " Height:" << h << " Format:" << fmt << "\n";

        pEnc = new NvEncoderD3D11(pD3DDev, Width, Height, BufferFormat);
        if (!pEnc)
        {
            returnIfError(E_FAIL);
        }

        ZeroMemory(&encInitParams, sizeof(encInitParams));
        encInitParams.encodeConfig = &encConfig;
        encInitParams.encodeWidth = Width;
        encInitParams.encodeHeight = Height;
        encInitParams.maxEncodeWidth = UHD_W;
        encInitParams.maxEncodeHeight = UHD_H;
        encInitParams.frameRateNum = MaxFPS;
        encInitParams.frameRateDen = 1;

        ZeroMemory(&encConfig, sizeof(encConfig));
        encConfig.gopLength = GoPLength;

        try
        {
            pEnc->CreateDefaultEncoderParams(&encInitParams, Codec_Id, NV_ENC_PRESET_LOW_LATENCY_HP_GUID);
            pEnc->CreateEncoder(&encInitParams);
        }
        catch (...)
        {
            returnIfError(E_FAIL);
        }
    }
    return S_OK;
}

CaptureRuntime::CaptureRuntime(const VideoCaptureSettings capture_settings, const H264CodecSettings codec_settings)
{
    Width = capture_settings.Width;
    Height = capture_settings.Height;
    MaxFPS = capture_settings.MaxFPS;

    Codec_Id = NV_ENC_CODEC_H264_GUID;
    BufferFormat = codec_settings.Format;
    GoPLength = codec_settings.GoPLength;
    CRF = codec_settings.CRF;
}

CaptureRuntime::~CaptureRuntime()
{
    //Clean up DXGI and D3D in here
}

HRESULT CaptureRuntime::Init()
{
    HRESULT hr = S_OK;

    hr = InitDXGI();
    returnIfError(hr);

    hr = InitDup();
    returnIfError(hr);

    hr = InitEnc();
    returnIfError(hr);
}
