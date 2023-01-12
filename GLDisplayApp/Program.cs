using FlaneerMediaLib;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace GLDisplayApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        CommandLineArgumentStore.CreateAndRegister(args);

        SmartStorageSubsystem.InitSmartStorage();
        
        NetworkSubsystem.InitClient();
        
        var videoSettings = new VideoSettings();
        InitialiseMediaEncoder();
        GLWindow window = new GLWindow(videoSettings.Width, videoSettings.Height);
        GLEnv env = new GLEnv(window);
        window.StartAppLoop();
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
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        var videoSource = clArgStore.HasArgument(CommandLineArgs.UseLocalFrames) ? VideoSource.TestH264 : VideoSource.UDPH264;
        
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(videoSource);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}
