using System.Runtime.InteropServices;

namespace FFmpegDecoderWrapper
{
    internal static class InteropMethods
    {
        private const string DLLNAME = "FFmpegDecoder.dll";

        [DllImport(DLLNAME)]
        public static extern void Init(VideoFrameSettings capture_settings, ref FrameRequest frame_request);

        [DllImport(DLLNAME)]
        public static extern bool FulfilFrameRequest(ref FrameRequest frame_request);

        [DllImport(DLLNAME)]
        public static extern void CleanUp();
    }
}
