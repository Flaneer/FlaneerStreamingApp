using FlaneerMediaLib;

namespace UDPToWinFormTest;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        InitialiseMediaEncoder();
        
        NReco.VideoConverter.License.SetLicenseKey(
            "Video_Converter_Bin_Examples_Pack_252489380218",
            "S/Q/c7WKOxHMTcFThcDRSiZHFghpLIAZQGgjdlQLdUmPIDYwgaQUUtjZ2mQeBMLJw/8Hqqt2J8d/wOydqyIAF/tpy/baitBPegJ6Js9CCl6JfImW/fDLmbE8IJcxT2APwRjmWfXJi+Qxc3RZLO/Lna0dCPCEcMoeH9zMLN9veeU="
        );
        
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    class VideoSettings
    {
        public int Height = 1440;
        public int Width = 2560;
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
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSources.UDPH264);
        encoderLifeCycleManager.InitVideo(frameSettings, codecSettings);
    }
}