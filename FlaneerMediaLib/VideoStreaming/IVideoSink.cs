namespace FlaneerMediaLib.VideoStreaming
{
    /// <summary>
    /// Base interface for all video sinks
    /// </summary>
    public interface IVideoSink
    {
        /// <summary>
        /// Process a single frame
        /// </summary>
        void ProcessFrame();
        /// <summary>
        /// Process a number of frames at a given rate
        /// </summary>
        void ProcessFrames(int numberOfFrames, int targetFramerate);
    }
}
