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

        using MediaEncoder encoder = new MediaEncoder(VideoEncoders.NvEncH264);

        if(encoder.InitVideo(frameSettings, codecSettings))
        {
            UDPVideoStreamer videoSink = new UDPVideoStreamer();
            videoSink.Capture(600, frameSettings.MaxFPS);
        }
        else
        {
            Console.WriteLine("Failed to init video");
            Console.ReadLine();
        }
        
        Console.WriteLine("Message sent to the broadcast address");
    }
}