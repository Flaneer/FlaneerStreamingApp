namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Args provided when a new frame is ready 
/// </summary>
internal class FrameReadyArgs : EventArgs
{
    /// <summary>
    /// The idx of this frame in the sequence
    /// </summary>
    public uint sequenceIdx;
    /// <summary>
    /// The frame data
    /// </summary>
    public UnassembledFrame unassembledFrame;
    /// <summary>
    /// Whether or not this is an i frame
    /// </summary>
    public bool isIFrame;
}
