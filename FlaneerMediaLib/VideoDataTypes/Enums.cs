namespace FlaneerMediaLib.VideoDataTypes;

public enum VideoSources
{
    NvEncH264,
    UDPH264
}

public enum VideoCodec
{
    H264,
    H265
}

public enum BufferFormat
{
    UNDEFINED,
    NV12,
    YV12,
    IYUV,
    YUV444,
    YUV420_10BIT,
    YUV444_10BIT,
    ARGB,
    ARGB10,
    AYUV,
    ABGR,
    ABGR10,
}