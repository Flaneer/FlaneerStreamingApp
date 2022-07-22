using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace UDPTestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InitialiseMediaEncoder();
        
            NReco.VideoConverter.License.SetLicenseKey(
                "Video_Converter_Bin_Examples_Pack_252489380218",
                "S/Q/c7WKOxHMTcFThcDRSiZHFghpLIAZQGgjdlQLdUmPIDYwgaQUUtjZ2mQeBMLJw/8Hqqt2J8d/wOydqyIAF/tpy/baitBPegJ6Js9CCl6JfImW/fDLmbE8IJcxT2APwRjmWfXJi+Qxc3RZLO/Lna0dCPCEcMoeH9zMLN9veeU="
            ); 
            UDPListener.StartListener();
            Console.ReadLine();
        }

        class VideoSettings
        {
            public readonly int Height = 1440;
            public readonly int Width = 2560;
            public readonly int MaxFPS = 60;
            public readonly BufferFormat Format = BufferFormat.ARGB;
            public readonly int GoPLength = 5;
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
}