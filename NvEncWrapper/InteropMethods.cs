using System.Runtime.InteropServices;

namespace NvEncWrapper
{
    internal static class InteropMethods
    {
        private const string DLLNAME = "nvencvideosource.dll";

        [DllImport(DLLNAME)]
        public static extern long Init(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings);

        [DllImport(DLLNAME)]
        public static extern long FulfilFrameRequest(FrameRequest frame_request);

        [DllImport(DLLNAME)]
        public static extern void CleanUp();
    }
}