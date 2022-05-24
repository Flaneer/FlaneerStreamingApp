using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;

namespace GLDisplayApp;

public class UDPImageSource
{
    private readonly IVideoSource videoSource;
    private readonly FFMpegDecoder videoConv;
    
    private Logger logger;
    
    private readonly short width;
    private readonly short height;

    public UDPImageSource()
    {
        logger = Logger.GetLogger(this);
        
        
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.FrameSettings);
        width =  Int16.Parse(frameSettings[0]);
        height = Int16.Parse(frameSettings[1]);
        
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
                Width = width,
                Height = height,
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
