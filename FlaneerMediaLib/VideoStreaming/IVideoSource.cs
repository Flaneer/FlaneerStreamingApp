using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    /// <summary>
    /// Base interface for all video sources
    /// </summary>
    public interface IVideoSource : IService, IDisposable
    {
        /// <summary>
        /// The codec settings
        /// </summary>
        ICodecSettings CodecSettings { get; }
        /// <summary>
        /// The frame settings
        /// </summary>
        FrameSettings FrameSettings { get; }
        
        /// <summary>
        /// Initialize the video source
        /// </summary>
        public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn);
        /// <summary>
        /// Get a frame from the video source
        /// </summary>
        /// <param name="frame">The frame if available</param>
        /// <returns>true if there is a frame available</returns>
         bool GetFrame(out IVideoFrame frame);
    }
}
