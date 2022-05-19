using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace GLDisplayApp;

public class UDPImageSource
{
    private readonly IVideoSource videoSource;
    private readonly FFMpegDecoder videoConv;
    
    private Logger logger = Logger.GetCurrentClassLogger();

    public UDPImageSource()
    {
        ServiceRegistry.TryGetService(out videoSource);
        videoConv = new FFMpegDecoder();
    }
    
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
            logger.Error(e);
        }

        return new ManagedVideoFrame();
    }
}