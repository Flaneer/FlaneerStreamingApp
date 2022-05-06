using System.Diagnostics;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
using NReco.VideoConverter;

namespace GLDisplayApp;

public class UDPImageSource
{
    private readonly IVideoSource videoSource;
    private readonly FFMpegDecoder videoConv;

    public UDPImageSource()
    {
        ServiceRegistry.TryGetService(out videoSource);
        videoConv = new FFMpegDecoder();
    }

    private int counter = 1;
    private string NextFrame => $"{counter * (1.0f/30)}";
    
    public ManagedVideoFrame GetImage()
    {
        try
        {
            var frameIn = videoSource.GetFrame();
            ManagedVideoFrame? frame = frameIn as ManagedVideoFrame;
            if(frame == null || frame.Stream.Length == 0)
                return new ManagedVideoFrame();

            if(!File.Exists("out.h264"))
                File.WriteAllBytes("out.h264",frame.Stream.ToArray());
            
            var frameOut = videoConv.DecodeFrame(frame.Stream);
            
            return new ManagedVideoFrame()
            {
                Width = 1920,
                Height = 1080,
                Stream = frameOut
            };
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }

        return new ManagedVideoFrame();
    }
}