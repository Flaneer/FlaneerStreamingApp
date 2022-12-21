using FFmpeg.AutoGen;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg;

internal class FrameInfo
{
    public AVPictureType PictType;
    public AVPixelFormat Format;
    public bool KeyFrame;
    public int CodedPictureNumber;
}
