using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
{
    /// <summary>
    /// Specific settings for H264 encoders
    /// </summary>
    public class H264CodecSettings : ICodecSettings
    {
        /// <summary>
        /// Length of the group of pictures
        /// </summary>
        public Int16 GoPLength;
        /// <summary>
        /// The colour format of the buffer
        /// </summary>
        public BufferFormat Format;
    }
}
