using FlaneerMediaLib;
using System.Text.Json;
using System.Windows;

namespace LocalMediaFileOut
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"The current directory is {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"NvEncVideoSource.dll exists: {File.Exists("NvEncVideoSource.dll")}");

            var videoSettings = await ProcessVideoSettings();

            using MediaEncoder encoder = new MediaEncoder(VideoEncoders.NvEncH264);

            var frameSettings = new FrameSettings()
            {
                Height = videoSettings.Height,
                Width = videoSettings.Width,
                MaxFPS = videoSettings.MaxFPS
            };

            var codecSettings = new H264CodecSettings()
            {
                Format = videoSettings.Format,
                GoPLength = (short)videoSettings.GoPLength
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

        class VideoSettings
        {
            public int Height = 1440;
            public int Width = 2560;
            public int MaxFPS = 60;
            public BufferFormat Format = BufferFormat.ARGB;
            public int GoPLength = 5;
        }

        private static async Task<VideoSettings> ProcessVideoSettings()
        {
            client.DefaultRequestHeaders.Accept.Clear();

            var streamTask = client.GetStreamAsync("https://d5r5xl46i4.execute-api.eu-west-1.amazonaws.com/ConfigDemoStage/");
            var VideoSettings = await JsonSerializer.DeserializeAsync<VideoSettings>(await streamTask);
            return VideoSettings;
        }
    }
}