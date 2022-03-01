using System.Runtime.InteropServices;

namespace NvEncWrapper
{
    /// <summary>
    /// The colour format of the buffer
    /// <remarks>Copy of FlaneerStreamingApp\NvEnc\nvEncodeAPI.NV_ENC_BUFFER_FORMAT</remarks>
    /// </summary>
    internal enum NV_BufferFormat
    {
        NV_ENC_BUFFER_FORMAT_UNDEFINED = 0x00000000,  /**< Undefined buffer format */

        NV_ENC_BUFFER_FORMAT_NV12 = 0x00000001,  /* Semi-Planar YUV [Y plane followed by interleaved UV plane] */
        
        NV_ENC_BUFFER_FORMAT_YV12 = 0x00000010,  /* Planar YUV [Y plane followed by V and U planes] */
        
        NV_ENC_BUFFER_FORMAT_IYUV = 0x00000100,  /* Planar YUV [Y plane followed by U and V planes] */
        
        NV_ENC_BUFFER_FORMAT_YUV444 = 0x00001000,  /* Planar YUV [Y plane followed by U and V planes] */
        
        NV_ENC_BUFFER_FORMAT_YUV420_10BIT = 0x00010000,  /* 10 bit Semi-Planar YUV [Y plane followed by interleaved UV plane]. Each pixel of size 2 bytes. Most Significant 10 bits contain pixel data. */
        
        NV_ENC_BUFFER_FORMAT_YUV444_10BIT = 0x00100000,  /* 10 bit Planar YUV444 [Y plane followed by U and V planes]. Each pixel of size 2 bytes. Most Significant 10 bits contain pixel data.  */
        
        NV_ENC_BUFFER_FORMAT_ARGB = 0x01000000,  /*  8 bit Packed A8R8G8B8. This is a word-ordered format
                                                     where a pixel is represented by a 32-bit word with B
                                                     in the lowest 8 bits, G in the next 8 bits, R in the
                                                     8 bits after that and A in the highest 8 bits. */

        NV_ENC_BUFFER_FORMAT_ARGB10 = 0x02000000,  /*10 bit Packed A2R10G10B10. This is a word-ordered format
                                                     where a pixel is represented by a 32-bit word with B
                                                     in the lowest 10 bits, G in the next 10 bits, R in the
                                                     10 bits after that and A in the highest 2 bits. */

        NV_ENC_BUFFER_FORMAT_AYUV = 0x04000000,  /* 8 bit Packed A8Y8U8V8. This is a word-ordered format
                                                     where a pixel is represented by a 32-bit word with V
                                                     in the lowest 8 bits, U in the next 8 bits, Y in the
                                                     8 bits after that and A in the highest 8 bits. */

        NV_ENC_BUFFER_FORMAT_ABGR = 0x10000000,  /* 8 bit Packed A8B8G8R8. This is a word-ordered format
                                                     where a pixel is represented by a 32-bit word with R
                                                     in the lowest 8 bits, G in the next 8 bits, B in the
                                                     8 bits after that and A in the highest 8 bits. */

        NV_ENC_BUFFER_FORMAT_ABGR10 = 0x20000000, /* 10 bit Packed A2B10G10R10. This is a word-ordered format
                                                     where a pixel is represented by a 32-bit word with R
                                                     in the lowest 10 bits, G in the next 10 bits, B in the
                                                     10 bits after that and A in the highest 2 bits. */
    }

    /// <summary>
    /// Settings that specify the specs fo the video capture feed
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct VideoCaptureSettings
    {
        public Int16 Width;
        public Int16 Height;

        public Int16 MaxFPS;
    }
    
    /// <summary>
    /// Provides information to configure 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct H264CodecSettings
    {
        public NV_BufferFormat Format;
        public Int16 GoPLength;
    }

    /// <summary>
    /// Given to the native lib with information to generate a frame and
    /// an empty data pointer to be set to the address of the frame data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FrameRequest
    {
        public Int16 Width;
        public Int16 Height;

        public IntPtr Data;
    }
}
