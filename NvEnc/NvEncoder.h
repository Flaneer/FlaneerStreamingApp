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

#pragma once

#include <vector>
#include "nvEncodeAPI.h"
#include <stdint.h>
#include <mutex>
#include <string>
#include <iostream>
#include <sstream>
#include <string.h>
#include "InteropStructs.h"
#include "Utils.h"

/**
* @brief Shared base class for different encoder interfaces.
*/
class NvEncoder
{
public:
    /**
    *  @brief This function is used to initialize the encoder session.
    *  Application must call this function to initialize the encoder, before
    *  starting to encode any frames.
    */
    void CreateEncoder(const NV_ENC_INITIALIZE_PARAMS* encodeParams);

    /**
    *  @brief  This function is used to destroy the encoder session.
    *  Application must call this function to destroy the encoder session and
    *  clean up any allocated resources. The application must call EndEncode()
    *  function to get any queued encoded frames before calling DestroyEncoder().
    */
    void DestroyEncoder();

    /**
    *  @brief  This function is used to reconfigure an existing encoder session.
    *  Application can use this function to dynamically change the bitrate,
    *  resolution and other QOS parameters. If the application changes the
    *  resolution, it must set NV_ENC_RECONFIGURE_PARAMS::forceIDR.
    */
    bool Reconfigure(const NV_ENC_RECONFIGURE_PARAMS *reconfigureParams);

    /**
    *  @brief  This function is used to get the next available input buffer.
    *  Applications must call this function to obtain a pointer to the next
    *  input buffer. The application must copy the uncompressed data to the
    *  input buffer and then call EncodeFrame() function to encode it.
    */
    const NvEncInputFrame* GetNextInputFrame();


    /**
    *  @brief  This function is used to encode a frame.
    *  Applications must call EncodeFrame() function to encode the uncompressed
    *  data, which has been copied to an input buffer obtained from the
    *  GetNextInputFrame() function.
    */
    void EncodeFrame(std::vector<std::vector<uint8_t>> &packet, NV_ENC_PIC_PARAMS *picParams = nullptr);

    /**
    *  @brief  This function to flush the encoder queue.
    *  The encoder might be queuing frames for B picture encoding or lookahead;
    *  the application must call EndEncode() to get all the queued encoded frames
    *  from the encoder. The application must call this function before destroying
    *  an encoder session.
    */
    void EndEncode(std::vector<std::vector<uint8_t>> &packet);

    /**
    *  @brief  This function is used to query hardware encoder capabilities.
    *  Applications can call this function to query capabilities like maximum encode
    *  dimensions, support for lookahead or the ME-only mode etc.
    */
    int GetCapabilityValue(GUID guidCodec, NV_ENC_CAPS capsToQuery);

    /**
    *  @brief  This function is used to get the current device on which encoder is running.
    */
    void *GetDevice() const { return m_device; }

    /**
    *  @brief  This function is used to get the current device type which encoder is running.
    */
    NV_ENC_DEVICE_TYPE GetDeviceType() const { return m_deviceType; }

    /**
    *  @brief  This function is used to get the current encode width.
    *  The encode width can be modified by Reconfigure() function.
    */
    int GetEncodeWidth() const { return m_width; }

    /**
    *  @brief  This function is used to get the current encode height.
    *  The encode height can be modified by Reconfigure() function.
    */
    int GetEncodeHeight() const { return m_height; }

    /**
    *   @brief  This function is used to get the current frame size based on pixel format.
    */
    int GetFrameSize() const;

    /**
    *  @brief  This function is used to initialize config parameters based on
    *          given codec and preset guids.
    *  The application can call this function to get the default configuration
    *  for a certain preset. The application can either use these parameters
    *  directly or override them with application-specific settings before
    *  using them in CreateEncoder() function.
    */
    void SetEncoderParams(NV_ENC_INITIALIZE_PARAMS* intializeParams, GUID codecGuid, GUID presetGuid);

    /**
    *  @brief  This function is used to get the current initialization parameters,
    *          which had been used to configure the encoder session.
    *  The initialization parameters are modified if the application calls
    *  Reconfigure() function.
    */
    void GetInitializeParams(NV_ENC_INITIALIZE_PARAMS *initializeParams);

    /**
    *  @brief  This function is used to run motion estimation
    *  This is used to run motion estimation on a a pair of frames. The
    *  application must copy the reference frame data to the buffer obtained
    *  by calling GetNextReferenceFrame(), and copy the input frame data to
    *  the buffer obtained by calling GetNextInputFrame() before calling the
    *  RunMotionEstimation() function.
    */
    void RunMotionEstimation(std::vector<uint8_t> &mvData);

    /**
    *  @brief This function is used to get an available reference frame.
    *  Application must call this function to get a pointer to reference buffer,
    *  to be used in the subsequent RunMotionEstimation() function.
    */
    const NvEncInputFrame* GetNextReferenceFrame();

    /**
    *  @brief This function is used to get sequence and picture parameter headers.
    *  Application can call this function after encoder is initialized to get SPS and PPS
    *  nalus for the current encoder instance. The sequence header data might change when
    *  application calls Reconfigure() function.
    */
    void GetSequenceParams(std::vector<uint8_t> &seqParams);

    /**
    *  @brief  NvEncoder class virtual destructor.
    */
    virtual ~NvEncoder();

public:
    /**
    *  @brief This a static function to get chroma offsets for YUV planar formats.
    */
    static void GetChromaSubPlaneOffsets(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t pitch,
                                        const uint32_t height, std::vector<uint32_t>& chromaOffsets);
    /**
    *  @brief This a static function to get the chroma plane pitch for YUV planar formats.
    */
    static uint32_t GetChromaPitch(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaPitch);

    /**
    *  @brief This a static function to get the number of chroma planes for YUV planar formats.
    */
    static uint32_t GetNumChromaPlanes(const NV_ENC_BUFFER_FORMAT bufferFormat);

    /**
    *  @brief This a static function to get the chroma plane width in bytes for YUV planar formats.
    */
    static uint32_t GetChromaWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaWidth);

    /**
    *  @brief This a static function to get the chroma planes height in bytes for YUV planar formats.
    */
    static uint32_t GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight);


    /**
    *  @brief This a static function to get the width in bytes for the frame.
    *  For YUV planar format this is the width in bytes of the luma plane.
    */
    static uint32_t GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width);

protected:

    /**
    *  @brief  NvEncoder class constructor.
    *  NvEncoder class constructor cannot be called directly by the application.
    */
    NvEncoder(NV_ENC_DEVICE_TYPE deviceType, void *device, EncInitSettings initSettings, uint32_t outputDelay, bool motionEstimationOnly);

    /**
    *  @brief This function is used to check if hardware encoder is properly initialized.
    */
    bool IsHWEncoderInitialized() const { return m_encoder != NULL && m_encoderInitialized; }

    /**
    *  @brief This function is used to register CUDA, D3D or OpenGL input buffers with NvEncodeAPI.
    *  This is non public function and is called by derived class for allocating
    *  and registering input buffers.
    */
    void RegisterResources(std::vector<void*> inputframes, NV_ENC_INPUT_RESOURCE_TYPE resourceType,
        int width, int height, int pitch, NV_ENC_BUFFER_FORMAT bufferFormat, bool referenceFrame = false);

    /**
    *  @brief This function is used to unregister resources which had been previously registered for encoding
    *         using RegisterResources() function.
    */
    void UnregisterResources();
    /**
    *  @brief This function returns maximum width used to open the encoder session.
    *  All encode input buffers are allocated using maximum dimensions.
    */
    uint32_t GetMaxEncodeWidth() const { return m_maxEncodeWidth; }

    /**
    *  @brief This function returns maximum height used to open the encoder session.
    *  All encode input buffers are allocated using maximum dimensions.
    */
    uint32_t GetMaxEncodeHeight() const { return m_maxEncodeHeight; }

    /**
    *  @brief This function returns the current pixel format.
    */
    NV_ENC_BUFFER_FORMAT GetPixelFormat() const { return m_bufferFormat; }

private:
    /**
    *  @brief This is a private function which is used to wait for completion of encode command.
    */
    void WaitForCompletionEvent(int iEvent);

    /**
    *  @brief This is a private function which is used to check if there is any
              buffering done by encoder.
    *  The encoder generally buffers data to encode B frames or for lookahead
    *  or pipelining.
    */
    bool IsZeroDelay() { return m_outputDelay == 0; }

    /**
    *  @brief This is a private function which is used to load the encode api shared library.
    */
    void LoadNvEncApi();

    /**
    *  @brief This is a private function which is used to submit the encode
    *         commands to the NVENC hardware.
    */
    void DoEncode(NV_ENC_INPUT_PTR inputBuffer, std::vector<std::vector<uint8_t>> &packet, NV_ENC_PIC_PARAMS *picParams);

    /**
    *  @brief This is a private function which is used to submit the encode
    *         commands to the NVENC hardware for ME only mode.
    */
    void DoMotionEstimation(NV_ENC_INPUT_PTR inputBuffer, NV_ENC_INPUT_PTR referenceFrame, std::vector<uint8_t> &mvData);

    /**
    *  @brief This is a private function which is used to get the output packets
    *         from the encoder HW.
    *  This is called by DoEncode() function. If there is buffering enabled,
    *  this may return without any output data.
    */
    void GetEncodedPacket(std::vector<NV_ENC_OUTPUT_PTR> &outputBuffer, std::vector<std::vector<uint8_t>> &packet, bool outputDelay);

    /**
    *  @brief This is a private function which is used to initialize the bitstream buffers.
    *  This is only used in the encoding mode.
    */
    void InitializeBitstreamBuffer();

    /**
    *  @brief This is a private function which is used to destroy the bitstream buffers.
    *  This is only used in the encoding mode.
    */
    void DestroyBitstreamBuffer();

    /**
    *  @brief This is a private function which is used to initialize MV output buffers.
    *  This is only used in ME-only Mode.
    */
    void InitializeMVOutputBuffer();

    /**
    *  @brief This is a private function which is used to destroy MV output buffers.
    *  This is only used in ME-only Mode.
    */
    void DestroyMVOutputBuffer();

    /**
    *  @brief This is a private function which is used to destroy HW encoder.
    */
    void DestroyHWEncoder();

private:
    /**
    *  @brief This is a pure virtual function which is used to allocate input buffers.
    *  The derived classes must implement this function.
    */
    virtual void AllocateInputBuffers(int32_t numInputBuffers) = 0;

    /**
    *  @brief This is a pure virtual function which is used to destroy input buffers.
    *  The derived classes must implement this function.
    */
    virtual void ReleaseInputBuffers() = 0;

protected:
    bool m_motionEstimationOnly = false;
    void *m_encoder = nullptr;
    NV_ENCODE_API_FUNCTION_LIST m_nvenc;
    std::vector<NvEncInputFrame> m_inputFrames;
    std::vector<NV_ENC_REGISTERED_PTR> m_registeredResources;
    std::vector<NvEncInputFrame> m_referenceFrames;
    std::vector<NV_ENC_REGISTERED_PTR> m_registeredResourcesForReference;
private:
    uint32_t m_width;
    uint32_t m_height;
    NV_ENC_BUFFER_FORMAT m_bufferFormat;
    void *m_device;
    NV_ENC_DEVICE_TYPE m_deviceType;
    NV_ENC_INITIALIZE_PARAMS m_initializeParams = {};
    NV_ENC_CONFIG m_encodeConfig = {};
    bool m_encoderInitialized = false;
    uint32_t m_extraOutputDelay = 3;
    std::vector<NV_ENC_INPUT_PTR> m_mappedInputBuffers;
    std::vector<NV_ENC_INPUT_PTR> m_mappedRefBuffers;
    std::vector<NV_ENC_OUTPUT_PTR> m_bitstreamOutputBuffer;
    std::vector<NV_ENC_OUTPUT_PTR> m_mvDataOutputBuffer;
    std::vector<void *> m_completionEvent;
    uint32_t m_maxEncodeWidth = 0;
    uint32_t m_maxEncodeHeight = 0;
    void* m_module = nullptr;
    int32_t m_toSend = 0;
    int32_t m_got = 0;
    int32_t m_encoderBuffer = 0;
    int32_t m_outputDelay = 0;
    int32_t m_gopLength = 5;
};
