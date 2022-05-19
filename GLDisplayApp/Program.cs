using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
using GLFWTestApp;

namespace GLDisplayApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        CommandLineArguementStore.CreateAndRegister(args);
        
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
        
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var videoSource = clas.HasArgument(CommandLineArgs.UseLocalFrames) ? VideoSource.TestH264 : VideoSource.UDPH264;
        
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(videoSource);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}
