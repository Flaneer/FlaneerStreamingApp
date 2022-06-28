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

#include "NvEncoder.h"

NvEncoder::NvEncoder(NV_ENC_DEVICE_TYPE deviceType, void *device, uint32_t width, uint32_t height, NV_ENC_BUFFER_FORMAT bufferFormat,
                            uint32_t extraOutputDelay, bool motionEstimationOnly) :
    m_device(device), 
    m_deviceType(deviceType),
    m_width(width),
    m_height(height),
    m_maxEncodeWidth(width),
    m_maxEncodeHeight(height),
    m_bufferFormat(bufferFormat), 
    m_motionEstimationOnly(motionEstimationOnly), 
    m_extraOutputDelay(extraOutputDelay), 
    m_encoder(nullptr)
{
    LoadNvEncApi();

    if (!m_nvenc.nvEncOpenEncodeSession) 
    {
        m_encoderBuffer = 0;
        NVENC_THROW_ERROR("EncodeAPI not found", NV_ENC_ERR_NO_ENCODE_DEVICE);
    }

    NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS encodeSessionExParams = { NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER };
    encodeSessionExParams.device = m_device;
    encodeSessionExParams.deviceType = m_deviceType;
    encodeSessionExParams.apiVersion = NVENCAPI_VERSION;
    void *hEncoder = NULL;
    NVENC_API_CALL(m_nvenc.nvEncOpenEncodeSessionEx(&encodeSessionExParams, &hEncoder));
    m_encoder = hEncoder;
}

void NvEncoder::LoadNvEncApi()
{
#if defined(_WIN32)
#if defined(_WIN64)
    HMODULE hModule = LoadLibrary(TEXT("nvEncodeAPI64.dll"));
#else
    HMODULE hModule = LoadLibrary(TEXT("nvEncodeAPI.dll"));
#endif
#else
    void *hModule = dlopen("libnvidia-encode.so.1", RTLD_LAZY);
#endif

    if (hModule == NULL)
    {
        NVENC_THROW_ERROR("NVENC library file is not found. Please ensure NV driver is installed", NV_ENC_ERR_NO_ENCODE_DEVICE);
    }

    m_module = hModule;

    typedef NVENCSTATUS(NVENCAPI *NvEncodeAPIGetMaxSupportedVersion_Type)(uint32_t*);
#if defined(_WIN32)
    NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion = (NvEncodeAPIGetMaxSupportedVersion_Type)GetProcAddress(hModule, "NvEncodeAPIGetMaxSupportedVersion");
#else
    NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion = (NvEncodeAPIGetMaxSupportedVersion_Type)dlsym(hModule, "NvEncodeAPIGetMaxSupportedVersion");
#endif

    uint32_t version = 0;
    uint32_t currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
    NVENC_API_CALL(NvEncodeAPIGetMaxSupportedVersion(&version));
    if (currentVersion > version)
    {
        NVENC_THROW_ERROR("Current Driver Version does not support this NvEncodeAPI version, please upgrade driver", NV_ENC_ERR_INVALID_VERSION);
    }

    typedef NVENCSTATUS(NVENCAPI *NvEncodeAPICreateInstance_Type)(NV_ENCODE_API_FUNCTION_LIST*);
#if defined(_WIN32)
    NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)GetProcAddress(hModule, "NvEncodeAPICreateInstance");
#else
    NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)dlsym(hModule, "NvEncodeAPICreateInstance");
#endif

    if (!NvEncodeAPICreateInstance)
    {
        NVENC_THROW_ERROR("Cannot find NvEncodeAPICreateInstance() entry in NVENC library", NV_ENC_ERR_NO_ENCODE_DEVICE);
    }

    m_nvenc = { NV_ENCODE_API_FUNCTION_LIST_VER };
    NVENC_API_CALL(NvEncodeAPICreateInstance(&m_nvenc));
}

NvEncoder::~NvEncoder()
{
    DestroyHWEncoder();

    if (m_module)
    {
#if defined(_WIN32)
        FreeLibrary((HMODULE)m_module);
#else
        dlclose(m_hModule);
#endif
        m_module = nullptr;
    }
}

void NvEncoder::CreateDefaultEncoderParams(NV_ENC_INITIALIZE_PARAMS* pIntializeParams, GUID codecGuid, GUID presetGuid)
{
    if (!m_encoder)
    {
        NVENC_THROW_ERROR("Encoder Initialization failed", NV_ENC_ERR_NO_ENCODE_DEVICE);
        return;
    }

    if (pIntializeParams == nullptr || pIntializeParams->encodeConfig == nullptr)
    {
        NVENC_THROW_ERROR("pInitializeParams and pInitializeParams->encodeConfig can't be NULL", NV_ENC_ERR_INVALID_PTR);
    }

    memset(pIntializeParams->encodeConfig, 0, sizeof(NV_ENC_CONFIG));
    auto pEncodeConfig = pIntializeParams->encodeConfig;
    memset(pIntializeParams, 0, sizeof(NV_ENC_INITIALIZE_PARAMS));
    pIntializeParams->encodeConfig = pEncodeConfig;


    pIntializeParams->encodeConfig->version = NV_ENC_CONFIG_VER;
    pIntializeParams->version = NV_ENC_INITIALIZE_PARAMS_VER;

    pIntializeParams->encodeGUID = codecGuid;
    pIntializeParams->presetGUID = presetGuid;
    pIntializeParams->encodeWidth = m_width;
    pIntializeParams->encodeHeight = m_height;
    pIntializeParams->darWidth = m_width;
    pIntializeParams->darHeight = m_height;
    pIntializeParams->frameRateNum = 60;
    pIntializeParams->frameRateDen = 1;
    pIntializeParams->enablePTD = 1;
    pIntializeParams->reportSliceOffsets = 0;
    pIntializeParams->enableSubFrameWrite = 0;
    pIntializeParams->maxEncodeWidth = m_width;
    pIntializeParams->maxEncodeHeight = m_height;
    pIntializeParams->enableMEOnlyMode = m_motionEstimationOnly;
#if defined(_WIN32)
    pIntializeParams->enableEncodeAsync = true;
#endif

    NV_ENC_PRESET_CONFIG presetConfig = { NV_ENC_PRESET_CONFIG_VER, { NV_ENC_CONFIG_VER } };
    m_nvenc.nvEncGetEncodePresetConfig(m_encoder, codecGuid, presetGuid, &presetConfig);
    memcpy(pIntializeParams->encodeConfig, &presetConfig.presetCfg, sizeof(NV_ENC_CONFIG));
    pIntializeParams->encodeConfig->frameIntervalP = 1;
    pIntializeParams->encodeConfig->gopLength = 1;

    //pIntializeParams->encodeConfig->rcParams.rateControlMode = NV_ENC_PARAMS_RC_CONSTQP;

    pIntializeParams->encodeConfig->rcParams.rateControlMode = NV_ENC_PARAMS_RC_VBR;
    //pIntializeParams->encodeConfig->rcParams.maxBitRate = 800000;
    pIntializeParams->encodeConfig->rcParams.zeroReorderDelay = 1;

    if (pIntializeParams->presetGUID != NV_ENC_PRESET_LOSSLESS_DEFAULT_GUID
        && pIntializeParams->presetGUID != NV_ENC_PRESET_LOSSLESS_HP_GUID)
    {
        pIntializeParams->encodeConfig->rcParams.constQP = { 28, 31, 25 };
    }

    if (pIntializeParams->encodeGUID == NV_ENC_CODEC_H264_GUID)
    {
        if (m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444 || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT)
        {
            pIntializeParams->encodeConfig->encodeCodecConfig.h264Config.chromaFormatIDC = 3;
        }
        pIntializeParams->encodeConfig->encodeCodecConfig.h264Config.idrPeriod = pIntializeParams->encodeConfig->gopLength;
    }
    else if (pIntializeParams->encodeGUID == NV_ENC_CODEC_HEVC_GUID)
    {
        pIntializeParams->encodeConfig->encodeCodecConfig.hevcConfig.pixelBitDepthMinus8 =
            (m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV420_10BIT || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT ) ? 2 : 0;
        if (m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444 || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT)
        {
            pIntializeParams->encodeConfig->encodeCodecConfig.hevcConfig.chromaFormatIDC = 3;
        }
        pIntializeParams->encodeConfig->encodeCodecConfig.hevcConfig.idrPeriod = pIntializeParams->encodeConfig->gopLength;
    }

    return;
}

void NvEncoder::CreateEncoder(const NV_ENC_INITIALIZE_PARAMS* pEncoderParams)
{
    if (!m_encoder)
    {
        NVENC_THROW_ERROR("Encoder Initialization failed", NV_ENC_ERR_NO_ENCODE_DEVICE);
    }

    if (!pEncoderParams)
    {
        NVENC_THROW_ERROR("Invalid NV_ENC_INITIALIZE_PARAMS ptr", NV_ENC_ERR_INVALID_PTR);
    }

    if (pEncoderParams->encodeWidth == 0 || pEncoderParams->encodeHeight == 0)
    {
        NVENC_THROW_ERROR("Invalid encoder width and height", NV_ENC_ERR_INVALID_PARAM);
    }

    if (pEncoderParams->encodeGUID != NV_ENC_CODEC_H264_GUID && pEncoderParams->encodeGUID != NV_ENC_CODEC_HEVC_GUID)
    {
        NVENC_THROW_ERROR("Invalid codec guid", NV_ENC_ERR_INVALID_PARAM);
    }

    if (pEncoderParams->encodeGUID == NV_ENC_CODEC_H264_GUID)
    {
        if (m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV420_10BIT || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT)
        {
            NVENC_THROW_ERROR("10-bit format isn't supported by H264 encoder", NV_ENC_ERR_INVALID_PARAM);
        }
    }

    // set other necessary params if not set yet
    if (pEncoderParams->encodeGUID == NV_ENC_CODEC_H264_GUID)
    {
        if ((m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444) &&
            (pEncoderParams->encodeConfig->encodeCodecConfig.h264Config.chromaFormatIDC != 3))
        {
            NVENC_THROW_ERROR("Invalid ChromaFormatIDC", NV_ENC_ERR_INVALID_PARAM);
        }
    }

    if (pEncoderParams->encodeGUID == NV_ENC_CODEC_HEVC_GUID)
    {
        bool yuv10BitFormat = (m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV420_10BIT || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT) ? true : false;
        if (yuv10BitFormat && pEncoderParams->encodeConfig->encodeCodecConfig.hevcConfig.pixelBitDepthMinus8 != 2)
        {
            NVENC_THROW_ERROR("Invalid PixelBitdepth", NV_ENC_ERR_INVALID_PARAM);
        }

        if ((m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444 || m_bufferFormat == NV_ENC_BUFFER_FORMAT_YUV444_10BIT) &&
            (pEncoderParams->encodeConfig->encodeCodecConfig.hevcConfig.chromaFormatIDC != 3))
        {
            NVENC_THROW_ERROR("Invalid ChromaFormatIDC", NV_ENC_ERR_INVALID_PARAM);
        }
    }

    memcpy(&m_initializeParams, pEncoderParams, sizeof(m_initializeParams));
    m_initializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;

    if (pEncoderParams->encodeConfig)
    {
        memcpy(&m_encodeConfig, pEncoderParams->encodeConfig, sizeof(m_encodeConfig));
        m_encodeConfig.version = NV_ENC_CONFIG_VER;
    }
    else
    {
        NV_ENC_PRESET_CONFIG presetConfig = { NV_ENC_PRESET_CONFIG_VER, { NV_ENC_CONFIG_VER } };
        m_nvenc.nvEncGetEncodePresetConfig(m_encoder, pEncoderParams->encodeGUID, NV_ENC_PRESET_DEFAULT_GUID, &presetConfig);
        memcpy(&m_encodeConfig, &presetConfig.presetCfg, sizeof(NV_ENC_CONFIG));
        m_encodeConfig.version = NV_ENC_CONFIG_VER;
        m_encodeConfig.rcParams.rateControlMode = NV_ENC_PARAMS_RC_CONSTQP;
        m_encodeConfig.rcParams.constQP = { 28, 31, 25 };
    }
    m_initializeParams.encodeConfig = &m_encodeConfig;

    NVENC_API_CALL(m_nvenc.nvEncInitializeEncoder(m_encoder, &m_initializeParams));

    m_encoderInitialized = true;
    m_width = m_initializeParams.encodeWidth;
    m_height = m_initializeParams.encodeHeight;
    m_maxEncodeWidth = m_initializeParams.maxEncodeWidth;
    m_maxEncodeHeight = m_initializeParams.maxEncodeHeight;

    m_encoderBuffer = m_encodeConfig.frameIntervalP + m_encodeConfig.rcParams.lookaheadDepth + m_extraOutputDelay;
    m_outputDelay = m_encoderBuffer - 1;
    m_mappedInputBuffers.resize(m_encoderBuffer, nullptr);

    m_completionEvent.resize(m_encoderBuffer, nullptr);
#if defined(_WIN32)
    for (int i = 0; i < m_encoderBuffer; i++) 
    {
        m_completionEvent[i] = CreateEvent(NULL, FALSE, FALSE, NULL);
        NV_ENC_EVENT_PARAMS eventParams = { NV_ENC_EVENT_PARAMS_VER };
        eventParams.completionEvent = m_completionEvent[i];
        m_nvenc.nvEncRegisterAsyncEvent(m_encoder, &eventParams);
    }
#endif

    if (m_motionEstimationOnly)
    {
        m_mappedRefBuffers.resize(m_encoderBuffer, nullptr);
        InitializeMVOutputBuffer();
    }
    else
    {
        m_bitstreamOutputBuffer.resize(m_encoderBuffer, nullptr);
        InitializeBitstreamBuffer();
    }

    AllocateInputBuffers(m_encoderBuffer);
}

void NvEncoder::DestroyEncoder()
{
    if (!m_encoder)
    {
        return;
    }

    ReleaseInputBuffers();

    DestroyHWEncoder();
}

void NvEncoder::DestroyHWEncoder()
{
    if (!m_encoder)
    {
        return;
    }

#if defined(_WIN32)
    for (uint32_t i = 0; i < m_completionEvent.size(); i++)
    {
        if (m_completionEvent[i])
        {
            NV_ENC_EVENT_PARAMS eventParams = { NV_ENC_EVENT_PARAMS_VER };
            eventParams.completionEvent = m_completionEvent[i];
            m_nvenc.nvEncUnregisterAsyncEvent(m_encoder, &eventParams);
            CloseHandle(m_completionEvent[i]);
        }
    }
    m_completionEvent.clear();
#endif

    if (m_motionEstimationOnly)
    {
        DestroyMVOutputBuffer();
    }
    else
    {
        DestroyBitstreamBuffer();
    }

    m_nvenc.nvEncDestroyEncoder(m_encoder);

    m_encoder = nullptr;

    m_encoderInitialized = false;
}

const NvEncInputFrame* NvEncoder::GetNextInputFrame()
{
    int i = m_toSend % m_encoderBuffer;
    return &m_inputFrames[i];
}

const NvEncInputFrame* NvEncoder::GetNextReferenceFrame()
{
    int i = m_toSend % m_encoderBuffer;
    return &m_referenceFrames[i];
}

void NvEncoder::EncodeFrame(std::vector<std::vector<uint8_t>> &vPacket, NV_ENC_PIC_PARAMS *pPicParams)
{
    vPacket.clear();
    if (!IsHWEncoderInitialized())
    {
        NVENC_THROW_ERROR("Encoder device not found", NV_ENC_ERR_NO_ENCODE_DEVICE);
    }
    int i = m_toSend % m_encoderBuffer;
    NV_ENC_MAP_INPUT_RESOURCE mapInputResource = { NV_ENC_MAP_INPUT_RESOURCE_VER };
    mapInputResource.registeredResource = m_registeredResources[i];
    NVENC_API_CALL(m_nvenc.nvEncMapInputResource(m_encoder, &mapInputResource));
    m_mappedInputBuffers[i] = mapInputResource.mappedResource;
    DoEncode(m_mappedInputBuffers[i], vPacket, pPicParams);
}

void NvEncoder::RunMotionEstimation(std::vector<uint8_t> &mvData)
{
    if (!m_encoder)
    {
        NVENC_THROW_ERROR("Encoder Initialization failed", NV_ENC_ERR_NO_ENCODE_DEVICE);
        return;
    }

    const uint32_t i = m_toSend % m_encoderBuffer;

    NV_ENC_MAP_INPUT_RESOURCE mapInputResource = { NV_ENC_MAP_INPUT_RESOURCE_VER };
    mapInputResource.registeredResource = m_registeredResources[i];
    NVENC_API_CALL(m_nvenc.nvEncMapInputResource(m_encoder, &mapInputResource));
    NV_ENC_INPUT_PTR pDeviceMemoryInputBuffer = mapInputResource.mappedResource;
    m_mappedInputBuffers[i] = mapInputResource.mappedResource;


    mapInputResource.registeredResource = m_registeredResourcesForReference[i];
    NVENC_API_CALL(m_nvenc.nvEncMapInputResource(m_encoder, &mapInputResource));
    NV_ENC_INPUT_PTR pDeviceMemoryInputBufferForReference = mapInputResource.mappedResource;
    m_mappedRefBuffers[i] = mapInputResource.mappedResource;

    DoMotionEstimation(pDeviceMemoryInputBuffer, pDeviceMemoryInputBufferForReference, mvData);
}


void NvEncoder::GetSequenceParams(std::vector<uint8_t> &seqParams)
{
    uint8_t spsppsData[1024]; // Assume maximum spspps data is 1KB or less
    memset(spsppsData, 0, sizeof(spsppsData));
    NV_ENC_SEQUENCE_PARAM_PAYLOAD payload = { NV_ENC_SEQUENCE_PARAM_PAYLOAD_VER };
    uint32_t spsppsSize = 0;

    payload.spsppsBuffer = spsppsData;
    payload.inBufferSize = sizeof(spsppsData);
    payload.outSPSPPSPayloadSize = &spsppsSize;
    NVENC_API_CALL(m_nvenc.nvEncGetSequenceParams(m_encoder, &payload));
    seqParams.clear();
    seqParams.insert(seqParams.end(), &spsppsData[0], &spsppsData[spsppsSize]);
}

void NvEncoder::DoEncode(NV_ENC_INPUT_PTR inputBuffer, std::vector<std::vector<uint8_t>> &vPacket, NV_ENC_PIC_PARAMS *pPicParams)
{
    NV_ENC_PIC_PARAMS picParams = {};
    if (pPicParams)
    {
        picParams = *pPicParams;
    }
    picParams.version = NV_ENC_PIC_PARAMS_VER;
    picParams.pictureStruct = NV_ENC_PIC_STRUCT_FRAME;
    picParams.inputBuffer = inputBuffer;
    picParams.bufferFmt = GetPixelFormat();
    picParams.inputWidth = GetEncodeWidth();
    picParams.inputHeight = GetEncodeHeight();
    picParams.outputBitstream = m_bitstreamOutputBuffer[m_toSend % m_encoderBuffer];
    picParams.completionEvent = m_completionEvent[m_toSend % m_encoderBuffer];
    NVENCSTATUS nvStatus = m_nvenc.nvEncEncodePicture(m_encoder, &picParams);
    if (nvStatus == NV_ENC_SUCCESS || nvStatus == NV_ENC_ERR_NEED_MORE_INPUT)
    {
        m_toSend++;
        GetEncodedPacket(m_bitstreamOutputBuffer, vPacket, true);
    }
    else
    {
        NVENC_THROW_ERROR("nvEncEncodePicture API failed", nvStatus);
    }
}

void NvEncoder::EndEncode(std::vector<std::vector<uint8_t>> &vPacket)
{
    vPacket.clear();
    if (!IsHWEncoderInitialized())
    {
        NVENC_THROW_ERROR("Encoder device not initialized", NV_ENC_ERR_ENCODER_NOT_INITIALIZED);
    }

    NV_ENC_PIC_PARAMS picParams = { NV_ENC_PIC_PARAMS_VER };
    picParams.encodePicFlags = NV_ENC_PIC_FLAG_EOS;
    picParams.completionEvent = m_completionEvent[m_toSend % m_encoderBuffer];
    NVENC_API_CALL(m_nvenc.nvEncEncodePicture(m_encoder, &picParams));
    GetEncodedPacket(m_bitstreamOutputBuffer, vPacket, false);
}

void NvEncoder::GetEncodedPacket(std::vector<NV_ENC_OUTPUT_PTR> &vOutputBuffer, std::vector<std::vector<uint8_t>> &vPacket, bool bOutputDelay)
{
    unsigned i = 0;
    int iEnd = m_toSend;
    for (; m_got < iEnd; m_got++)
    {
        WaitForCompletionEvent(m_got % m_encoderBuffer);
        NV_ENC_LOCK_BITSTREAM lockBitstreamData = { NV_ENC_LOCK_BITSTREAM_VER };
        lockBitstreamData.outputBitstream = vOutputBuffer[m_got % m_encoderBuffer];
        lockBitstreamData.doNotWait = false;
        NVENC_API_CALL(m_nvenc.nvEncLockBitstream(m_encoder, &lockBitstreamData));
  
        uint8_t *pData = (uint8_t *)lockBitstreamData.bitstreamBufferPtr;
        if (vPacket.size() < i + 1)
        {
            vPacket.push_back(std::vector<uint8_t>());
        }
        vPacket[i].clear();
        vPacket[i].insert(vPacket[i].end(), &pData[0], &pData[lockBitstreamData.bitstreamSizeInBytes]);
        i++;

        NVENC_API_CALL(m_nvenc.nvEncUnlockBitstream(m_encoder, lockBitstreamData.outputBitstream));

        if (m_mappedInputBuffers[m_got % m_encoderBuffer])
        {
            NVENC_API_CALL(m_nvenc.nvEncUnmapInputResource(m_encoder, m_mappedInputBuffers[m_got % m_encoderBuffer]));
            m_mappedInputBuffers[m_got % m_encoderBuffer] = nullptr;
        }

        if (m_motionEstimationOnly && m_mappedRefBuffers[m_got % m_encoderBuffer])
        {
            NVENC_API_CALL(m_nvenc.nvEncUnmapInputResource(m_encoder, m_mappedRefBuffers[m_got % m_encoderBuffer]));
            m_mappedRefBuffers[m_got % m_encoderBuffer] = nullptr;
        }
    }
}

bool NvEncoder::Reconfigure(const NV_ENC_RECONFIGURE_PARAMS *pReconfigureParams)
{
    NVENC_API_CALL(m_nvenc.nvEncReconfigureEncoder(m_encoder, const_cast<NV_ENC_RECONFIGURE_PARAMS*>(pReconfigureParams)));

    memcpy(&m_initializeParams, &(pReconfigureParams->reInitEncodeParams), sizeof(m_initializeParams));
    if (pReconfigureParams->reInitEncodeParams.encodeConfig)
    {
        memcpy(&m_encodeConfig, pReconfigureParams->reInitEncodeParams.encodeConfig, sizeof(m_encodeConfig));
    }

    m_width = m_initializeParams.encodeWidth;
    m_height = m_initializeParams.encodeHeight;
    m_maxEncodeWidth = m_initializeParams.maxEncodeWidth;
    m_maxEncodeHeight = m_initializeParams.maxEncodeHeight;

    return true;
}

void NvEncoder::RegisterResources(std::vector<void*> inputframes, NV_ENC_INPUT_RESOURCE_TYPE eResourceType,
                                         int width, int height, int pitch, NV_ENC_BUFFER_FORMAT bufferFormat, bool bReferenceFrame)
{
    for (uint32_t i = 0; i < inputframes.size(); ++i)
    {
        NV_ENC_REGISTER_RESOURCE registerResource = { NV_ENC_REGISTER_RESOURCE_VER };
        registerResource.resourceType = eResourceType;
        registerResource.resourceToRegister = (void *)inputframes[i];
        registerResource.width = width;
        registerResource.height = height;
        registerResource.pitch = pitch;
        registerResource.bufferFormat = bufferFormat;
        NVENC_API_CALL(m_nvenc.nvEncRegisterResource(m_encoder, &registerResource));

        std::vector<uint32_t> _chromaOffsets;
        NvEncoder::GetChromaSubPlaneOffsets(bufferFormat, pitch, height, _chromaOffsets);
        NvEncInputFrame inputframe = {};
        inputframe.inputPtr = (void *)inputframes[i];
        inputframe.chromaOffsets[0] = 0;
        inputframe.chromaOffsets[1] = 0;
        for (uint32_t ch = 0; ch < _chromaOffsets.size(); ch++)
        {
            inputframe.chromaOffsets[ch] = _chromaOffsets[ch];
        }
        inputframe.numChromaPlanes = NvEncoder::GetNumChromaPlanes(bufferFormat);
        inputframe.pitch = pitch;
        inputframe.chromaPitch = NvEncoder::GetChromaPitch(bufferFormat, pitch);
        inputframe.bufferFormat = bufferFormat;
        inputframe.resourceType = eResourceType;

        if (bReferenceFrame)
        {
            m_registeredResourcesForReference.push_back(registerResource.registeredResource);
            m_referenceFrames.push_back(inputframe);
        }
        else
        {
            m_registeredResources.push_back(registerResource.registeredResource);
            m_inputFrames.push_back(inputframe);
        }
    }
}

void NvEncoder::UnregisterResources()
{
    if (!m_motionEstimationOnly)
    {
        // Incase of error it is possible for buffers still mapped to encoder.
        // flush the encoder queue and then unmapped it if any surface is still mapped
        try
        {
            std::vector<std::vector<uint8_t>> vPacket;
            EndEncode(vPacket);
        }
        catch (...)
        {

        }
    }
    else
    {
        for (uint32_t i = 0; i < m_mappedRefBuffers.size(); ++i)
        {
            if (m_mappedRefBuffers[i])
            {
                m_nvenc.nvEncUnmapInputResource(m_encoder, m_mappedRefBuffers[i]);
            }
        }
    }
    m_mappedRefBuffers.clear();

    for (uint32_t i = 0; i < m_mappedInputBuffers.size(); ++i)
    {
        if (m_mappedInputBuffers[i])
        {
            m_nvenc.nvEncUnmapInputResource(m_encoder, m_mappedInputBuffers[i]);
        }
    }
    m_mappedInputBuffers.clear();

    for (uint32_t i = 0; i < m_registeredResources.size(); ++i)
    {
        if (m_registeredResources[i])
        {
            m_nvenc.nvEncUnregisterResource(m_encoder, m_registeredResources[i]);
        }
    }
    m_registeredResources.clear();


    for (uint32_t i = 0; i < m_registeredResourcesForReference.size(); ++i)
    {
        if (m_registeredResourcesForReference[i])
        {
            m_nvenc.nvEncUnregisterResource(m_encoder, m_registeredResourcesForReference[i]);
        }
    }
    m_registeredResourcesForReference.clear();

}


void NvEncoder::WaitForCompletionEvent(int iEvent)
{
#if defined(_WIN32)
#ifdef DEBUG
    WaitForSingleObject(m_vpCompletionEvent[iEvent], INFINITE);
#else
    // wait for 20s which is infinite on terms of gpu time
    if (WaitForSingleObject(m_completionEvent[iEvent], 20000) == WAIT_FAILED)
    {
        NVENC_THROW_ERROR("Failed to encode frame", NV_ENC_ERR_GENERIC);
    }
#endif
#endif
}

uint32_t NvEncoder::GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width)
{
    switch (bufferFormat) {
    case NV_ENC_BUFFER_FORMAT_NV12:
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
    case NV_ENC_BUFFER_FORMAT_YUV444:
        return width;
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return width * 2;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return width * 4;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return 0;
    }
}

uint32_t NvEncoder::GetNumChromaPlanes(const NV_ENC_BUFFER_FORMAT bufferFormat)
{
    switch (bufferFormat) 
    {
    case NV_ENC_BUFFER_FORMAT_NV12:
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
        return 1;
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
    case NV_ENC_BUFFER_FORMAT_YUV444:
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return 2;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return 0;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return -1;
    }
}

uint32_t NvEncoder::GetChromaPitch(const NV_ENC_BUFFER_FORMAT bufferFormat,const uint32_t lumaPitch)
{
    switch (bufferFormat)
    {
    case NV_ENC_BUFFER_FORMAT_NV12:
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
    case NV_ENC_BUFFER_FORMAT_YUV444:
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return lumaPitch;
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
        return (lumaPitch + 1)/2;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return 0;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return -1;
    }
}

void NvEncoder::GetChromaSubPlaneOffsets(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t pitch, const uint32_t height, std::vector<uint32_t>& chromaOffsets)
{
    chromaOffsets.clear();
    switch (bufferFormat)
    {
    case NV_ENC_BUFFER_FORMAT_NV12:
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
        chromaOffsets.push_back(pitch * height);
        return;
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
        chromaOffsets.push_back(pitch * height);
        chromaOffsets.push_back(chromaOffsets[0] + (NvEncoder::GetChromaPitch(bufferFormat, pitch) * GetChromaHeight(bufferFormat, height)));
        return;
    case NV_ENC_BUFFER_FORMAT_YUV444:
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        chromaOffsets.push_back(pitch * height);
        chromaOffsets.push_back(chromaOffsets[0] + (pitch * height));
        return;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return;
    }
}

uint32_t NvEncoder::GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight)
{
    switch (bufferFormat)
    {
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
    case NV_ENC_BUFFER_FORMAT_NV12:
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
        return (lumaHeight + 1)/2;
    case NV_ENC_BUFFER_FORMAT_YUV444:
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return lumaHeight;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return 0;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return 0;
    }
}

uint32_t NvEncoder::GetChromaWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaWidth)
{
    switch (bufferFormat)
    {
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
        return (lumaWidth + 1) / 2;
    case NV_ENC_BUFFER_FORMAT_NV12:
        return lumaWidth;
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
        return 2 * lumaWidth;
    case NV_ENC_BUFFER_FORMAT_YUV444:
        return lumaWidth;
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return 2 * lumaWidth;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return 0;
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return 0;
    }
}


int NvEncoder::GetCapabilityValue(GUID guidCodec, NV_ENC_CAPS capsToQuery)
{
    if (!m_encoder)
    {
        return 0;
    }
    NV_ENC_CAPS_PARAM capsParam = { NV_ENC_CAPS_PARAM_VER };
    capsParam.capsToQuery = capsToQuery;
    int v;
    m_nvenc.nvEncGetEncodeCaps(m_encoder, guidCodec, &capsParam, &v);
    return v;
}

int NvEncoder::GetFrameSize() const
{
    switch (GetPixelFormat())
    {
    case NV_ENC_BUFFER_FORMAT_YV12:
    case NV_ENC_BUFFER_FORMAT_IYUV:
    case NV_ENC_BUFFER_FORMAT_NV12:
        return GetEncodeWidth() * (GetEncodeHeight() + (GetEncodeHeight() + 1) / 2);
    case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
        return 2 * GetEncodeWidth() * (GetEncodeHeight() + (GetEncodeHeight() + 1) / 2);
    case NV_ENC_BUFFER_FORMAT_YUV444:
        return GetEncodeWidth() * GetEncodeHeight() * 3;
    case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
        return 2 * GetEncodeWidth() * GetEncodeHeight() * 3;
    case NV_ENC_BUFFER_FORMAT_ARGB:
    case NV_ENC_BUFFER_FORMAT_ARGB10:
    case NV_ENC_BUFFER_FORMAT_AYUV:
    case NV_ENC_BUFFER_FORMAT_ABGR:
    case NV_ENC_BUFFER_FORMAT_ABGR10:
        return 4 * GetEncodeWidth() * GetEncodeHeight();
    default:
        NVENC_THROW_ERROR("Invalid Buffer format", NV_ENC_ERR_INVALID_PARAM);
        return 0;
    }
}

void NvEncoder::GetInitializeParams(NV_ENC_INITIALIZE_PARAMS *pInitializeParams)
{
    if (!pInitializeParams || !pInitializeParams->encodeConfig)
    {
        NVENC_THROW_ERROR("Both pInitializeParams and pInitializeParams->encodeConfig can't be NULL", NV_ENC_ERR_INVALID_PTR);
    }
    NV_ENC_CONFIG *pEncodeConfig = pInitializeParams->encodeConfig;
    *pEncodeConfig = m_encodeConfig;
    *pInitializeParams = m_initializeParams;
    pInitializeParams->encodeConfig = pEncodeConfig;
}

void NvEncoder::InitializeBitstreamBuffer()
{
    for (int i = 0; i < m_encoderBuffer; i++)
    {
        NV_ENC_CREATE_BITSTREAM_BUFFER createBitstreamBuffer = { NV_ENC_CREATE_BITSTREAM_BUFFER_VER };
        NVENC_API_CALL(m_nvenc.nvEncCreateBitstreamBuffer(m_encoder, &createBitstreamBuffer));
        m_bitstreamOutputBuffer[i] = createBitstreamBuffer.bitstreamBuffer;
    }
}

void NvEncoder::DestroyBitstreamBuffer()
{
    for (uint32_t i = 0; i < m_bitstreamOutputBuffer.size(); i++)
    {
        if (m_bitstreamOutputBuffer[i])
        {
            m_nvenc.nvEncDestroyBitstreamBuffer(m_encoder, m_bitstreamOutputBuffer[i]);
        }
    }

    m_bitstreamOutputBuffer.clear();
}

void NvEncoder::InitializeMVOutputBuffer()
{
    for (int i = 0; i < m_encoderBuffer; i++)
    {
        NV_ENC_CREATE_MV_BUFFER createMVBuffer = { NV_ENC_CREATE_MV_BUFFER_VER };
        NVENC_API_CALL(m_nvenc.nvEncCreateMVBuffer(m_encoder, &createMVBuffer));
        m_mvDataOutputBuffer.push_back(createMVBuffer.mvBuffer);
    }
}

void NvEncoder::DestroyMVOutputBuffer()
{
    for (uint32_t i = 0; i < m_mvDataOutputBuffer.size(); i++)
    {
        if (m_mvDataOutputBuffer[i])
        {
            m_nvenc.nvEncDestroyMVBuffer(m_encoder, m_mvDataOutputBuffer[i]);
        }
    }

    m_mvDataOutputBuffer.clear();
}

void NvEncoder::DoMotionEstimation(NV_ENC_INPUT_PTR inputBuffer, NV_ENC_INPUT_PTR inputBufferForReference, std::vector<uint8_t> &mvData)
{
    NV_ENC_MEONLY_PARAMS meParams = { NV_ENC_MEONLY_PARAMS_VER };
    meParams.inputBuffer = inputBuffer;
    meParams.referenceFrame = inputBufferForReference;
    meParams.inputWidth = GetEncodeWidth();
    meParams.inputHeight = GetEncodeHeight();
    meParams.mvBuffer = m_mvDataOutputBuffer[m_toSend % m_encoderBuffer];
    meParams.completionEvent = m_completionEvent[m_toSend % m_encoderBuffer];
    NVENCSTATUS nvStatus = m_nvenc.nvEncRunMotionEstimationOnly(m_encoder, &meParams);
    if (nvStatus == NV_ENC_SUCCESS || nvStatus == NV_ENC_ERR_NEED_MORE_INPUT)
    {
        m_toSend++;
        std::vector<std::vector<uint8_t>> vPacket;
        GetEncodedPacket(m_mvDataOutputBuffer, vPacket, true);
        if (vPacket.size() != 1)
        {
            NVENC_THROW_ERROR("GetEncodedPacket() doesn't return one (and only one) MVData", NV_ENC_ERR_GENERIC);
        }
        mvData = vPacket[0];
    }
    else
    {
        NVENC_THROW_ERROR("nvEncEncodePicture API failed", nvStatus);
    }
}
