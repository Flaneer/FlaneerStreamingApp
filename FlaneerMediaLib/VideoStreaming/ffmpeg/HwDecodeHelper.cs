using FFmpeg.AutoGen;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{

    internal class HwDecodeHelper
    {
        public static AVHWDeviceType GetHWDecoder()
        {
            AVHWDeviceType HWtype;
            var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();

            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var number = 0;

            while ((type = FF.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                availableHWDecoders.Add(number, type);
                number++;
            }

            if (availableHWDecoders.Count == 0)
                return AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

            //Prefer AV_HWDEVICE_TYPE_DXVA2
            var decoderNumber = availableHWDecoders.SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
            //else use the first suggested HW decoder
            if (decoderNumber == 0)
                decoderNumber = availableHWDecoders.First().Key;
            
            if (availableHWDecoders.TryGetValue(decoderNumber, out HWtype))
                return HWtype;

            return AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        }
        
        public static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
        {
            return hWDevice switch
            {
                AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
                AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
                AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
                AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
                AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
                AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
                AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
                AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
                _ => AVPixelFormat.AV_PIX_FMT_NONE
            };
        }
    }
}
