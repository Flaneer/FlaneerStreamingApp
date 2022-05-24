using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

class Program
{
    class VideoSettings
    {
        public int Height = 1440;
        public int Width = 2560;
        public int MaxFPS = 60;
        public BufferFormat Format = BufferFormat.ARGB;
        public int GoPLength = 5;
    }
    
    static void Main(string[] args)
    {
        CommandLineArguementStore.CreateAndRegister(args);
     
        InitialiseMediaEncoder();

        IVideoSink videoSink = new UDPVideoSink();
        var videoSettings = new VideoSettings();
        videoSink.ProcessFrames(600, videoSettings.MaxFPS);
        
        Console.WriteLine("Message sent to the broadcast address");
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
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.NvEncH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}
