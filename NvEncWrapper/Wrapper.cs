namespace NvEncWrapper
{
    public class Wrapper
    {
        public static bool Init(VideoCaptureSettings capture_settings, H264CodecSettings codec_settings)
        {
            var retCode = InteropMethods.Init(capture_settings, codec_settings);
            return retCode == 0;
        }

        public static FrameRequest RequestNewFrame(int width, int height)
        {
            var interopFrame = new FrameRequest
            {
                Height = (Int16) height,
                Width = (Int16) width
            };
            var retCode = InteropMethods.FulfilFrameRequest(ref interopFrame);
            if (retCode == 0)
                return interopFrame;
            else
                throw new Exception($"Frame request failed, ret code {retCode}");
        }

        public static void CleanUp()
        {
            InteropMethods.CleanUp();
        }
    }
}
