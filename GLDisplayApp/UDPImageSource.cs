using System.Net.Sockets;
using FFmpeg.AutoGen;
using FlaneerMediaLib;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming.ffmpeg;

namespace GLDisplayApp;

public class UDPImageSource
{
    private readonly IVideoSource videoSource;
    
    private Logger logger;
    
    private readonly short width;
    private readonly short height;

    private MemoryStream? outFrameStream;

    private bool ffmpegInitialised = false;
    private VideoStreamDecoder? vsd;
    private AVIOReader? avioReader;
    private VideoFrameConverter? vfc;

    public UDPImageSource()
    {
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.FrameSettings);
        width =  Int16.Parse(frameSettings[0]);
        height = Int16.Parse(frameSettings[1]);
    
        ServiceRegistry.TryGetService(out videoSource);
    }

    private unsafe void InitialiseFFMpeg(ref MemoryStream? inFrameStream)
    {
        FFmpegBinariesHelper.RegisterFFmpegBinaries();
        avioReader = new AVIOReader(inFrameStream);
        vsd = new VideoStreamDecoder(avioReader.AvioCtx);
        var destinationSize = vsd.SourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
        vfc = new VideoFrameConverter(vsd.SourceSize, vsd.SourcePixelFormat, destinationSize, destinationPixelFormat);
        ffmpegInitialised = true;
    }

    public ManagedVideoFrame GetImage()
    {
        try
        {
            unsafe
            {
                var frameIn = videoSource.GetFrame();
                ManagedVideoFrame? frame = frameIn as ManagedVideoFrame;
                if(frame == null || frame.Stream.Length == 0)
                    return new ManagedVideoFrame();

                if(!File.Exists("out.h264"))
                    File.WriteAllBytes("out.h264",frame.Stream.ToArray());

                var inStream = frame.Stream;
                
                if(!ffmpegInitialised)
                    InitialiseFFMpeg(ref inStream);
                else
                    avioReader!.RefreshInputStream(inStream);
                
                var convertedFrame = vfc!.Convert(vsd!.DecodeNextFrame());
                //var frameOut = videoConv.DecodeFrame(frame.Stream);
                outFrameStream = new MemoryStream();
                outFrameStream.Write(new Span<byte>(convertedFrame.data[0], convertedFrame.height * convertedFrame.linesize[0]));
                return new ManagedVideoFrame()
                {
                    Width = width,
                    Height = height,
                    Stream = outFrameStream
                };
            }
        }
        catch (SocketException e)
        {
            logger.Error(e);
        }

        return new ManagedVideoFrame();
    }
}
