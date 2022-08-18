using FFmpeg.AutoGen;
using System.Runtime.InteropServices;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    internal class FFmpegHelper
    {
        public static unsafe string AVErr(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            FF.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }
    }
}
