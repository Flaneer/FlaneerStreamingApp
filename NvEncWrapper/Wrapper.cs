using FlaneerMediaLib;

namespace NvEncWrapper
{
    public class Wrapper
    {
        public static void Init(FrameSettings capture_settings, ICodecSettings codec_settings)
        {
            switch (codec_settings)
            {
                case FlaneerMediaLib.H264CodecSettings h264CodecSettings:
                    InteropMethods.Init(Utils.FromFrameSettings(capture_settings), Utils.FromCodecSettings(codec_settings));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codec_settings));
            }
        }

        public static IntPtr FulfilFrameRequest(int width, int height)
        {
            var interopFrame = new FrameRequest
            {
                Height = (Int16) height,
                Width = (Int16) width
            };
            InteropMethods.FulfilFrameRequest(interopFrame);
            return interopFrame.Data;
        }

        public static void CleanUp()
        {
            InteropMethods.CleanUp();
        }
    }
}
