/*
* Copyright 2017-2018 NVIDIA Corporation.  All rights reserved.
*
* Please refer to the NVIDIA end user license agreement (EULA) associated
* with this source code for terms and conditions that govern your use of
* this software. Any use, reproduction, disclosure, or distribution of
* this software and related documentation outside the terms of the EULA
* is strictly prohibited.
*
*/

#include "NvEncoderD3D11.h"
#include <d3d9types.h>

#ifndef MAKEFOURCC
#define MAKEFOURCC(a,b,c,d) (((unsigned int)a) | (((unsigned int)b)<< 8) | (((unsigned int)c)<<16) | (((unsigned int)d)<<24) )
#endif
#include "InteropStructs.h"

DXGI_FORMAT GetD3D11Format(NV_ENC_BUFFER_FORMAT bufferFormat)
{
    switch (bufferFormat)
    {
    case NV_ENC_BUFFER_FORMAT_NV12:
        return DXGI_FORMAT_NV12;
    case NV_ENC_BUFFER_FORMAT_ARGB:
        return DXGI_FORMAT_B8G8R8A8_UNORM;
    default:
        return DXGI_FORMAT_UNKNOWN;
    }
}

NvEncoderD3D11::NvEncoderD3D11(ID3D11Device* D3D11Device, EncInitSettings initSettings,  uint32_t extraOutputDelay, bool motionEstimationOnly) :
    NvEncoder(NV_ENC_DEVICE_TYPE_DIRECTX, D3D11Device, initSettings, extraOutputDelay, motionEstimationOnly)
{
    if (!D3D11Device)
    {
        NVENC_THROW_ERROR("Bad d3d11device ptr", NV_ENC_ERR_INVALID_PTR);
        return;
    }

    if (GetD3D11Format(GetPixelFormat()) == DXGI_FORMAT_UNKNOWN)
    {
        NVENC_THROW_ERROR("Unsupported Buffer format", NV_ENC_ERR_INVALID_PARAM);
    }

    if (!m_encoder)
    {
        NVENC_THROW_ERROR("Encoder Initialization failed", NV_ENC_ERR_INVALID_DEVICE);
    }

    m_d3d11Device = D3D11Device;
    m_d3d11Device->AddRef();
    m_d3d11Device->GetImmediateContext(&m_d3d11DeviceContext);
}

NvEncoderD3D11::~NvEncoderD3D11() 
{
    ReleaseD3D11Resources();
}

void NvEncoderD3D11::AllocateInputBuffers(int32_t numInputBuffers)
{
    if (!IsHWEncoderInitialized())
    {
        NVENC_THROW_ERROR("Encoder intialization failed", NV_ENC_ERR_ENCODER_NOT_INITIALIZED);
    }

    // for MEOnly mode we need to allocate seperate set of buffers for reference frame
    int numCount = m_motionEstimationOnly ? 2 : 1;
    for (int count = 0; count < numCount; count++)
    {
        std::vector<void*> inputFrames;
        for (int i = 0; i < numInputBuffers; i++)
        {
            ID3D11Texture2D *pInputTextures = NULL;
            D3D11_TEXTURE2D_DESC desc;
            ZeroMemory(&desc, sizeof(D3D11_TEXTURE2D_DESC));
            desc.Width = GetMaxEncodeWidth();
            desc.Height = GetMaxEncodeHeight();
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = GetD3D11Format(GetPixelFormat());
            desc.SampleDesc.Count = 1;
            desc.Usage = D3D11_USAGE_DEFAULT;
            desc.BindFlags = D3D11_BIND_RENDER_TARGET;
            desc.CPUAccessFlags = 0;
            if (m_d3d11Device->CreateTexture2D(&desc, NULL, &pInputTextures) != S_OK)
            {
                NVENC_THROW_ERROR("Failed to create d3d11textures", NV_ENC_ERR_OUT_OF_MEMORY);
            }
            inputFrames.push_back(pInputTextures);
        }
        RegisterResources(inputFrames, NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX, 
            GetMaxEncodeWidth(), GetMaxEncodeHeight(), 0, GetPixelFormat(), count == 1 ? true : false);
    }
}

void NvEncoderD3D11::ReleaseInputBuffers()
{
    ReleaseD3D11Resources();
}

void NvEncoderD3D11::ReleaseD3D11Resources()
{
    if (!m_encoder)
    {
        return;
    }

    UnregisterResources();

    for (uint32_t i = 0; i < m_inputFrames.size(); ++i)
    {
        if (m_inputFrames[i].inputPtr)
        {
            reinterpret_cast<ID3D11Texture2D*>(m_inputFrames[i].inputPtr)->Release();
        }
    }
    m_inputFrames.clear();

    for (uint32_t i = 0; i < m_referenceFrames.size(); ++i)
    {
        if (m_referenceFrames[i].inputPtr)
        {
            reinterpret_cast<ID3D11Texture2D*>(m_referenceFrames[i].inputPtr)->Release();
        }
    }
    m_referenceFrames.clear();

    if (m_d3d11DeviceContext)
    {
        m_d3d11DeviceContext->Release();
        m_d3d11DeviceContext = nullptr;
    }

    if (m_d3d11Device)
    {
        m_d3d11Device->Release();
        m_d3d11Device = nullptr;
    }
}

