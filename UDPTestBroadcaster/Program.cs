using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
using GLDisplayApp;

class Program
{
    static void Main(string[] args)
    {
        CommandLineArguementStore.CreateAndRegister(args);
     
        var videoSettings = new VideoSettings();
        InitialiseMediaEncoder(videoSettings);

        IVideoSink videoSink = new UDPVideoSink();
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
