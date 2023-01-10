using FlaneerMediaLib;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace UDPTestBroadcaster;

class Program
{
    static void Main(string[] args)
    {
        var logger = Logger.GetLogger(new object());
     
        logger.Trace("Building command line argument store");
        CommandLineArgumentStore.CreateAndRegister(args);
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        
        logger.Trace("Initializing Smart Storage");
        SmartStorageSubsystem.InitSmartStorage();
        logger.Trace("Initializing Server");
        NetworkSubsystem.InitServer();

        ServiceRegistry.TryGetService(out UDPSender udpSender);

        logger.Trace("Waiting for peer to be registered");
        while (!udpSender.PeerRegistered && !clas.HasArgument(CommandLineArgs.NoNet))
        {
            Thread.Sleep(500);
        }
        logger.Trace("Peer registered");
        
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
