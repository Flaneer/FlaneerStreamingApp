using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace GLDisplayApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        CommandLineArgumentStore.CreateAndRegister(args);
        
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

        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.ffmpegH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}
