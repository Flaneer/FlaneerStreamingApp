namespace FlaneerMediaLib.VideoStreaming.ffmpeg;

internal class FrameInfo
{
    public char PictType;
    public int Format;
    public long Pts;
    public int KeyFrame;
    public int CodedPictureNumber;
    public int PktSize;
}
