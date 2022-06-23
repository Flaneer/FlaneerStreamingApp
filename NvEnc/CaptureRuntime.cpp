#include "CaptureRuntime.h"

#include "DDAImpl.h"
#include "Defs.h"
#include "nvEncodeAPI.h"
#include "NvEncoderD3D11.h"

CaptureRuntime::CaptureRuntime(const VideoCaptureSettings capture_settings, const H264CodecSettings codec_settings)
{
    m_width = capture_settings.Width;
    m_height = capture_settings.Height;
    m_maxFPS = capture_settings.MaxFPS;

    m_codecId = NV_ENC_CODEC_H264_GUID;
    m_bufferFormat = codec_settings.Format;
    m_gopLength = codec_settings.GoPLength;
}

CaptureRuntime::~CaptureRuntime()
{
    Cleanup();
}

std::vector <IDXGIAdapter*> GetNVidiaAdapter(void)
{
    IDXGIAdapter* adapter;
    std::vector <IDXGIAdapter*> adapters;
    IDXGIFactory* factory = nullptr;

    // Create a DXGIFactory object.
    if (FAILED(CreateDXGIFactory(__uuidof(IDXGIFactory), reinterpret_cast<void**>(&factory))))
    {
        return adapters;
    }

    for (UINT i = 0; factory->EnumAdapters(i, &adapter) != DXGI_ERROR_NOT_FOUND; ++i)
    {
	    DXGI_ADAPTER_DESC desc;
        adapter->GetDesc(&desc);
        if(desc.VendorId == 4318)//4318 is NVidia's vendor ID
        adapters.push_back(adapter);
    }

    if (factory)
    {
        factory->Release();
    }

    return adapters;
}

/// Initialize DXGI pipeline
HRESULT CaptureRuntime::InitDXGI()
{
    HRESULT hr = S_FALSE;
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
        auto adapters = GetNVidiaAdapter();
	    
        if(adapters.empty())
        {
            break;
        }

        //hr = D3D11CreateDevice(
        //    adapters[0],//Get the first NVidia adapter
        //    D3D_DRIVER_TYPE_UNKNOWN,
        //    nullptr,
        //    /*D3D11_CREATE_DEVICE_DEBUG*/0, 
        //    FeatureLevels, 
        //    NumFeatureLevels,
        //    D3D11_SDK_VERSION, 
        //    &D3DDevice, &FeatureLevel, &deviceContext);

        hr = D3D11CreateDevice(
            nullptr,
            DriverTypes[DriverTypeIndex],
            nullptr,
            /*D3D11_CREATE_DEVICE_DEBUG*/0,
            FeatureLevels,
            NumFeatureLevels,
            D3D11_SDK_VERSION,
            &m_d3dDevice, &FeatureLevel, &m_deviceContext);
        
	    if (SUCCEEDED(hr))
	    {
	        // Device creation succeeded, no need to loop anymore
            return hr;
	    }
    }
    return hr;
}

/// Initialize DDA handler
HRESULT CaptureRuntime::InitDup()
{
    HRESULT hr = S_OK;
    if (!m_ddaWrapper)
    {
        m_ddaWrapper = new DDAImpl(m_d3dDevice, m_deviceContext);
        hr = m_ddaWrapper->Init();
        returnIfError(hr);
    }
    return hr;
}

/// Initialize NVENCODEAPI wrapper
HRESULT CaptureRuntime::InitEnc()
{
    if (!m_encoder)
    {
        //std::cout << "Video details: Width:" << w << " Height:" << h << " Format:" << fmt << "\n";

        m_encoder = new NvEncoderD3D11(m_d3dDevice, m_width, m_height, m_bufferFormat);
        if (!m_encoder)
        {
            returnIfError(E_FAIL);
        }

        ZeroMemory(&m_encInitParams, sizeof(m_encInitParams));
        m_encInitParams.encodeConfig = &m_encConfig;
        m_encInitParams.encodeWidth = m_width;
        m_encInitParams.encodeHeight = m_height;
        m_encInitParams.maxEncodeWidth = UHD_W;
        m_encInitParams.maxEncodeHeight = UHD_H;
        m_encInitParams.frameRateNum = m_maxFPS;
        m_encInitParams.frameRateDen = 1;

        ZeroMemory(&m_encConfig, sizeof(m_encConfig));
        m_encConfig.gopLength = m_gopLength;

        try
        {
            m_encoder->CreateDefaultEncoderParams(&m_encInitParams, m_codecId, NV_ENC_PRESET_LOW_LATENCY_HP_GUID);
            m_encoder->CreateEncoder(&m_encInitParams);
        }
        catch (...)
        {
            returnIfError(E_FAIL);
        }
    }
    return S_OK;
}

/// Initialize preprocessor
HRESULT CaptureRuntime::InitColorConv()
{
    if (!m_colorConv)
    {
        m_colorConv = new RGBToNV12(m_d3dDevice, m_deviceContext);
        HRESULT hr = m_colorConv->Init();
        returnIfError(hr);
    }
    return S_OK;
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

    hr = InitColorConv();
    returnIfError(hr);

    return hr;
}

/// Capture a frame using DDA
HRESULT CaptureRuntime::Capture()
{
    if (m_ddaWrapper)
        return m_ddaWrapper->GetCapturedFrame(&m_dupTex2D, timeout()); // Release after preproc
    else
        return 1;
}

/// Preprocess captured frame
HRESULT CaptureRuntime::Preproc()
{
    HRESULT hr = S_OK;
    const NvEncInputFrame* pEncInput = m_encoder->GetNextInputFrame();
    m_encBuf = (ID3D11Texture2D*)pEncInput->inputPtr;
    hr = m_colorConv->Convert(m_dupTex2D, m_encBuf);
    SAFE_RELEASE(m_dupTex2D);
    returnIfError(hr);

    m_encBuf->AddRef();  // Release after encode
    return hr;
}

/// Encode the captured frame using NVENCODEAPI
HRESULT CaptureRuntime::Encode()
{
    HRESULT hr = S_OK;
    try
    {
        m_encoder->EncodeFrame(m_packet);
    }
    catch (...)
    {
        hr = E_FAIL;
    }
    SAFE_RELEASE(m_encBuf);
    return hr;
}

HRESULT CaptureRuntime::FulfilFrameRequest(FrameRequest& frame_request)
{
    auto hr = Capture();
    if (FAILED(hr))
    {
        return hr;
    }
    /// Preprocess for encoding
    hr = Preproc();
    if (FAILED(hr))
    {
        printf("Preproc failed with error 0x%08x\n", hr);
        return hr;
    }
    hr = Encode();
    if (FAILED(hr))
    {
        printf("Encode failed with error 0x%08x\n", hr);
        return hr;
    }
    //Casting here because interop is best done with explicit size types
    frame_request.Buffersize = (INT32)m_packet.back().size();
    frame_request.Data = m_packet.back().data();
    return hr;
}

void CaptureRuntime::Cleanup()
{
    if (m_ddaWrapper)
    {
        m_ddaWrapper->Cleanup();
        delete m_ddaWrapper;
        m_ddaWrapper = nullptr;
    }

    if (m_colorConv)
    {
        m_colorConv->Cleanup();
    }

    if (m_encoder)
    {
        ZeroMemory(&m_encInitParams, sizeof(NV_ENC_INITIALIZE_PARAMS));
        ZeroMemory(&m_encConfig, sizeof(NV_ENC_CONFIG));
    }

    SAFE_RELEASE(m_dupTex2D);
    if (m_encoder)
    {
        /// Flush the encoder and write all output to file before destroying the encoder
        /*encoder->EndEncode(vPacket);
        WriteEncOutput();
        encoder->DestroyEncoder();
        if (bDelete)
        {
            delete encoder;
            encoder = nullptr;
        }*/

        ZeroMemory(&m_encInitParams, sizeof(NV_ENC_INITIALIZE_PARAMS));
        ZeroMemory(&m_encConfig, sizeof(NV_ENC_CONFIG));
    }

    SAFE_RELEASE(m_d3dDevice);
    SAFE_RELEASE(m_deviceContext);
}