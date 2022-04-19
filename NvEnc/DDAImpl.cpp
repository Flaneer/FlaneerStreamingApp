/*
 * Copyright (c) 2019, NVIDIA CORPORATION. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *  * Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *  * Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *  * Neither the name of NVIDIA CORPORATION nor the names of its
 *    contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "Defs.h"
#include "DDAImpl.h"
#include <iomanip>

/// Initialize DDA
HRESULT DDAImpl::Init()
{
    IDXGIOutput * output = nullptr;
    IDXGIDevice2* device = nullptr;
    IDXGIAdapter *adapter = nullptr;
    IDXGIOutput1* out1 = nullptr;

    /// Release all temporary refs before exit
#define CLEAN_RETURN(x) \
    SAFE_RELEASE(device);\
    SAFE_RELEASE(output);\
    SAFE_RELEASE(out1);\
    SAFE_RELEASE(adapter);\
    return x;

    HRESULT hr = S_OK;

    if (FAILED(hr = m_d3dDev->QueryInterface(__uuidof(IDXGIDevice2), (void**)&device)))
    {
        CLEAN_RETURN(hr);
    }

    if (FAILED(hr = device->GetParent(__uuidof(IDXGIAdapter), (void**)&adapter)))
    {
        CLEAN_RETURN(hr);
    }
    /// Once we have the DXGI Adapter, we enumerate the attached display outputs, and select which one we want to capture
    /// This sample application always captures the primary display output, enumerated at index 0.
    if (FAILED(hr = adapter->EnumOutputs(0, &output)))
    {
        CLEAN_RETURN(hr);
    }

    if (FAILED(hr = output->QueryInterface(__uuidof(IDXGIOutput1), (void**)&out1)))
    {
        CLEAN_RETURN(hr);
    }
    /// Ask DXGI to create an instance of IDXGIOutputDuplication for the selected output. We can now capture this display output
    if (FAILED(hr = out1->DuplicateOutput(device, &m_outputDuplication)))
    {
        CLEAN_RETURN(hr);
    }

    DXGI_OUTDUPL_DESC outDesc;
    ZeroMemory(&outDesc, sizeof(outDesc));
    m_outputDuplication->GetDesc(&outDesc);

    m_height = outDesc.ModeDesc.Height;
    m_width = outDesc.ModeDesc.Width;
    CLEAN_RETURN(hr);
}

/// Acquire a new frame from DDA, and return it as a Texture2D object.
/// 'wait' specifies the time in milliseconds that DDA shoulo wait for a new screen update.
HRESULT DDAImpl::GetCapturedFrame(ID3D11Texture2D **tex2D, int wait)
{
    HRESULT hr = S_OK;
    DXGI_OUTDUPL_FRAME_INFO frameInfo;
    ZeroMemory(&frameInfo, sizeof(frameInfo));
    int acquired = 0;
    

#define RETURN_ERR(x) {printf(__FUNCTION__": %d : Line %d return 0x%x\n", m_framenum, __LINE__, x);return x;}

    if (m_resource)
    {
        m_outputDuplication->ReleaseFrame();
        m_resource->Release();
        m_resource = nullptr;
    }

    hr = m_outputDuplication->AcquireNextFrame(wait, &frameInfo, &m_resource);
    if (FAILED(hr))
    {
        if (hr == DXGI_ERROR_WAIT_TIMEOUT)
        {
            printf(__FUNCTION__": %d : Wait for %d ms timed out\n", m_framenum, wait);
        }
        if (hr == DXGI_ERROR_INVALID_CALL)
        {
            printf(__FUNCTION__": %d : Invalid Call, previous frame not released?\n", m_framenum);
        }
        if (hr == DXGI_ERROR_ACCESS_LOST)
        {
            printf(__FUNCTION__": %d : Access lost, frame needs to be released?\n", m_framenum);
        }
        RETURN_ERR(hr);
    }
    if (frameInfo.AccumulatedFrames == 0 || frameInfo.LastPresentTime.QuadPart == 0)
    {
        // No image update, only cursor moved.
        m_ofs << "frameNo: " << m_framenum << " | Accumulated: " << frameInfo.AccumulatedFrames << "MouseOnly?" << frameInfo.LastMouseUpdateTime.QuadPart << endl;
        RETURN_ERR(DXGI_ERROR_WAIT_TIMEOUT);
    }

    if (!m_resource)
    {
        printf(__FUNCTION__": %d : Null output resource. Return error.\n", m_framenum);
        return E_UNEXPECTED;
    }

    if (FAILED(hr = m_resource->QueryInterface(__uuidof(ID3D11Texture2D), (void**)tex2D)))
    {
        return hr;
    }

    LARGE_INTEGER pts = frameInfo.LastPresentTime;  MICROSEC_TIME(pts, m_qpcFreq);
    LONGLONG interval = pts.QuadPart - m_lastPTS.QuadPart;

    printf(__FUNCTION__": %d : Accumulated Frames %u PTS Interval %lld PTS %lld\n", m_framenum, frameInfo.AccumulatedFrames,  interval * 1000, frameInfo.LastPresentTime.QuadPart);
    m_ofs << "frameNo: " << m_framenum << " | Accumulated: "<< frameInfo.AccumulatedFrames <<" | PTS: " << frameInfo.LastPresentTime.QuadPart << " | PTSInterval: "<< (interval)*1000<<endl;
    m_lastPTS = pts; // store microsec value
    m_framenum += frameInfo.AccumulatedFrames;
    return hr;
}

/// Release all resources
int DDAImpl::Cleanup()
{
    if (m_resource)
    {
        m_outputDuplication->ReleaseFrame();
        SAFE_RELEASE(m_resource);
    }

    m_width = m_height = m_framenum = 0;

    SAFE_RELEASE(m_outputDuplication);
    SAFE_RELEASE(m_devCtx);
    SAFE_RELEASE(m_d3dDev);

    return 0;
}