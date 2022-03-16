using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FlaneerMediaLib;
using LocalMediaFileOut;

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
        InitialiseMediaEncoder();

        UDPVideoStreamer videoSink = new UDPVideoStreamer();
        var videoSettings = new VideoSettings();
        videoSink.Capture(600, videoSettings.MaxFPS);
        
        Console.WriteLine("Message sent to the broadcast address");
    }

    private static void InitialiseMediaEncoder()
    {
        var videoSettings = new VideoSettings();
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
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoEncoders.NvEncH264);
        encoderLifeCycleManager.InitVideo(frameSettings, codecSettings);
    }
}