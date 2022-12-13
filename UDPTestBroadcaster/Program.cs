using FlaneerMediaLib;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace UDPTestBroadcaster;

class Program
{
    static void Main(string[] args)
    {
        CommandLineArgumentStore.CreateAndRegister(args);
        
        SmartStorageSubsystem.InitSmartStorage();
        
        NetworkSubsystem.InitServer();

        var videoSettings = new VideoSettings();
        InitialiseMediaEncoder(videoSettings);

        IVideoSink videoSink = new UDPVideoSink(videoSettings);
        videoSink.ProcessFrames(600, videoSettings.MaxFPS);
        
        Console.WriteLine("Message sent to the broadcast address");
    }

    private static void InitialiseMediaEncoder(VideoSettings videoSettings)
    {
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
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.NvEncH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}
