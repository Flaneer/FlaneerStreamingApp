using FlaneerMediaLib;

namespace LocalMediaFileOut
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"The current directory is {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"NvEncVideoSource.dll exists: {File.Exists("NvEncVideoSource.dll")}");

            using MediaEncoder encoder = new MediaEncoder(VideoEncoders.NvEncH264);

            var frameSettings = new FrameSettings()
            {
                Height = 1440,
                Width = 2560,
                MaxFPS = 60
            };

            var codecSettings = new H264CodecSettings()
            {
                Format = BufferFormat.ARGB,
                GoPLength = 5
            };

            if(encoder.InitVideo(frameSettings, codecSettings))
            {
                MP4VideoSink videoSink = new MP4VideoSink();
                videoSink.Capture(600, frameSettings.MaxFPS);
            }
            else
            {
                Console.WriteLine("Failed to init video");
                Console.ReadLine();
            }
        }
    }
}