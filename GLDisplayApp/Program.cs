using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace GLFWTestApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        InitialiseMediaEncoder();
        GLWindow window = new GLWindow(1920, 1080);
        GLEnv env = new GLEnv(window);
        window.StartAppLoop();
    }

    class VideoSettings
    {
        public int Height = 1080;
        public int Width = 1920;
        public int MaxFPS = 60;
        public BufferFormat Format = BufferFormat.ARGB;
        public int GoPLength = 5;
    }

    private static void InitialiseMediaEncoder()
    {
        var videoSettings = new VideoSettings();
        var frameSettings = new FrameSettings()
        {
            Height = (short) videoSettings.Height,
            Width = (short) videoSettings.Width,
            MaxFPS = videoSettings.MaxFPS
        };

        var codecSettings = new H264CodecSettings()
        {
            Format = videoSettings.Format,
            GoPLength = (short)videoSettings.GoPLength
        };
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.UDPH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}