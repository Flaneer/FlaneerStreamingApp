namespace NvEncWrapper
{
    public class Wrapper
    {
        public static bool Init(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings)
        {
            return InteropMethods.Init(capture_settings, codec_settings) == 0;
        }

        public static IntPtr RequestNewFrame(int width, int height)
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
