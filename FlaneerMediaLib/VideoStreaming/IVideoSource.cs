using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
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
        /// Returns a frame from the video source
        /// </summary>
        IVideoFrame GetFrame();
    }
}
