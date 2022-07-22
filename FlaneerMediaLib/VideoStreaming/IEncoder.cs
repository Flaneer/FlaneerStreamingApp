using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    /// <summary>
    /// Interface for all encoders
    /// </summary>
    public interface IEncoder : IService
    {
        /// <summary>
        /// Returns a frame from the encoder
        /// </summary>
        IVideoFrame GetFrame();
    }
}
