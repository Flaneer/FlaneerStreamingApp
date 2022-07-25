#pragma once
/**
* @brief Exception class for error reporting from NvEncodeAPI calls.
*/
class NVENCException : public std::exception
{
public:
    NVENCException(const std::string& errorStr, const NVENCSTATUS errorCode)
        : m_errorString(errorStr), m_errorCode(errorCode) {}

    virtual ~NVENCException() throw() {}
    virtual const char* what() const throw() { return m_errorString.c_str(); }
    NVENCSTATUS  getErrorCode() const { return m_errorCode; }
    const std::string& getErrorString() const { return m_errorString; }
    static NVENCException makeNVENCException(const std::string& errorStr, const NVENCSTATUS errorCode,
        const std::string& functionName, const std::string& fileName, int lineNo);
private:
    std::string m_errorString;
    NVENCSTATUS m_errorCode;
};

inline NVENCException NVENCException::makeNVENCException(const std::string& errorStr, const NVENCSTATUS errorCode, const std::string& functionName,
    const std::string& fileName, int lineNo)
{
    std::ostringstream errorLog;
    errorLog << functionName << " : " << errorStr << " at " << fileName << ":" << lineNo << std::endl;
    NVENCException exception(errorLog.str(), errorCode);
    return exception;
}

#define NVENC_THROW_ERROR( errorStr, errorCode )                                                         \
    do                                                                                                   \
    {                                                                                                    \
        throw NVENCException::makeNVENCException(errorStr, errorCode, __FUNCTION__, __FILE__, __LINE__); \
    } while (0)


#define NVENC_API_CALL( nvencAPI )                                                                                 \
    do                                                                                                             \
    {                                                                                                              \
        NVENCSTATUS errorCode = nvencAPI;                                                                          \
        if( errorCode != NV_ENC_SUCCESS)                                                                           \
        {                                                                                                          \
            std::ostringstream errorLog;                                                                           \
            errorLog << #nvencAPI << " returned error " << errorCode;                                              \
            throw NVENCException::makeNVENCException(errorLog.str(), errorCode, __FUNCTION__, __FILE__, __LINE__); \
        }                                                                                                          \
    } while (0)

struct NvEncInputFrame
{
    void* inputPtr = nullptr;
    uint32_t chromaOffsets[2];
    uint32_t numChromaPlanes;
    uint32_t pitch;
    uint32_t chromaPitch;
    NV_ENC_BUFFER_FORMAT bufferFormat;
    NV_ENC_INPUT_RESOURCE_TYPE resourceType;
};

/// <summary>
/// Settings that specify the specs fo the video capture feed
/// </summary>
typedef struct
{
    uint32_t Width;
    uint32_t Height;

    uint32_t MaxFPS;

    NV_ENC_BUFFER_FORMAT Format;
    uint32_t GoPLength;
} EncInitSettings;