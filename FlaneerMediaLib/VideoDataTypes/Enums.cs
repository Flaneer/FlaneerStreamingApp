//Disabling the normal naming/commenting rules since this file contains a lot of self explanatory acronyms that set off all the warnings

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
#pragma warning disable CS1591

namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// Potential video sources to inform the factory
/// </summary>
public enum VideoSource
{
    NvEncH264,
    UDPH264,
    TestH264
}

/// <summary>
/// Usable video codecs
/// </summary>
public enum VideoCodec
{
    H264,
    H265
}

/// <summary>
/// The colour format of the video buffer
/// </summary>
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