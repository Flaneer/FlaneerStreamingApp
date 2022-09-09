namespace FFmpegDecoderWrapper
{
    public class Wrapper
    {
        public static void Init(VideoFrameSettings capture_settings)
        {
            InteropMethods.Init(capture_settings);
        }

        public static FrameRequest RequestNewFrame(IntPtr DataIn, Int32 BufferSizeIn, int width, int height)
        {
            var interopFrame = new FrameRequest
            {
                Height = (Int16) height,
                Width = (Int16) width,
                DataIn = DataIn,
                BufferSizeIn = BufferSizeIn
            };
            var ret = InteropMethods.FulfilFrameRequest(ref interopFrame);
            if (ret)
                return interopFrame;
            
            throw new Exception($"Frame request failed");
        }

        public static void CleanUp()
        {
            InteropMethods.CleanUp();
        }
    }
}
