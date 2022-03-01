using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NvEncWrapper
{
    internal static class InteropMethods
    {
        private const string DLLNAME = "nvencvideosource.dll";

        [DllImport(DLLNAME)]
        public static extern void Init(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings);

        [DllImport(DLLNAME)]
        public static extern void FulfilFrameRequest(FrameRequest frame_request);

        [DllImport(DLLNAME)]
        public static extern void CleanUp();
    }
}