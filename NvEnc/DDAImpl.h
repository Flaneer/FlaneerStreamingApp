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

#pragma once
#include <iostream>
#include <vector>
#include <fstream>
using namespace std;
#include <dxgi1_2.h>
#include <d3d11_2.h>

class DDAImpl
{
    ///  Thin wrapper around IDXGIOutputDuplication interface
    /// Manages IDXGIOutputDuplication object lifecycle
    /// Interacts with IDXGIOuputDuplication to acquire new frames

private:
    /// The DDA object
    IDXGIOutputDuplication *m_outputDuplication = nullptr;
    /// The D3D11 device used by the DDA session
    ID3D11Device *m_d3dDev = nullptr;
    /// The D3D11 Device Context used by the DDA session
    ID3D11DeviceContext *m_devCtx = nullptr;
    /// The resource used to acquire a new captured frame from DDA
    IDXGIResource *m_resource = nullptr;
    /// Output width obtained from DXGI_OUTDUPL_DESC
    DWORD m_width = 0;
    /// Output height obtained from DXGI_OUTDUPL_DESC
    DWORD m_height = 0;
    /// Running count of no. of accumulated desktop updates
    int m_framenum = 0;
    /// output file stream to dump timestamps
    ofstream m_ofs;
    /// DXGI_OUTDUPL_FRAME_INFO::latPresentTime from the last Acquired frame
    LARGE_INTEGER m_lastPTS = { 0 };
    /// Clock frequency from QueryPerformaceFrequency()
    LARGE_INTEGER m_qpcFreq = { 0 };
    /// Default constructor
    DDAImpl() {}
    
public:
    /// Initialize DDA
    HRESULT Init();
    /// Acquire a new frame from DDA, and return it as a Texture2D object.
    /// 'wait' specifies the time in milliseconds that DDA shoulo wait for a new screen update.
    HRESULT GetCapturedFrame(ID3D11Texture2D **tex2D, int wait);
    /// Release all resources
    int Cleanup();
    /// Return output height to caller
    inline DWORD getWidth() { return m_width; }
    /// Return output width to caller
    inline DWORD getHeight() { return m_height; }


    /// Constructor
    DDAImpl(ID3D11Device *devIn, ID3D11DeviceContext* devCtxIn)
        :   m_d3dDev(devIn)
        ,   m_devCtx(devCtxIn)
    {
        m_d3dDev->AddRef();
        m_devCtx->AddRef();
        m_ofs = ofstream("PresentTSLog.txt");
        QueryPerformanceFrequency(&m_qpcFreq);
    }
    /// Destructor. Release all resources before destroying the object
    ~DDAImpl() { Cleanup(); }
};
