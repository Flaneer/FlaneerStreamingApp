using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
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
