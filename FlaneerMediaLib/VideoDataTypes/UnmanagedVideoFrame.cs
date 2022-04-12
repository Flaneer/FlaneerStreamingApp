namespace FlaneerMediaLib.VideoDataTypes;

public class UnmanagedVideoFrame : VideoFrame
{
    public IntPtr FrameData;
    public int FrameSize;
}