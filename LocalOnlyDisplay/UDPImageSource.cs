using System.Net.Sockets;
using FFmpeg.AutoGen;
using FlaneerMediaLib;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;
using FlaneerMediaLib.VideoStreaming.ffmpeg;

namespace GLDisplayApp;

public class UDPImageSource
{
    private readonly IVideoSource videoSource;
    
    private readonly Logger logger;
    
    private readonly short width;
    private readonly short height;

    private bool ffmpegInitialised;
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

    private unsafe void InitialiseFFMpeg(ref MemoryStream inFrameStream)
    {
        FFmpegLauncher.InitialiseFFMpeg();
        avioReader = new AVIOReader(inFrameStream);
        vsd = new VideoStreamDecoder(avioReader.AvioCtx);
        var destinationSize = vsd.SourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
        vfc = new VideoFrameConverter(vsd.SourceSize, vsd.SourcePixelFormat, destinationSize, destinationPixelFormat);
        ffmpegInitialised = true;
    }

    public UnsafeUnmanagedVideoFrame GetImage()
    {
        try
        {
            unsafe
            {
                var frameAvailable = videoSource.GetFrame(out var frameIn);
                if(!frameAvailable)
                    return new UnsafeUnmanagedVideoFrame();

                var unmanagedFrame = frameIn as UnmanagedVideoFrame;
                if (unmanagedFrame == null)
                    throw new Exception("Trying to use wrong frame type in ImageDecode");

                var frameStream = new MemoryStream(unmanagedFrame.FrameSize);
                frameStream.Write(new Span<byte>((byte*)unmanagedFrame.FrameData, unmanagedFrame.FrameSize));
                
                var frame = new ManagedVideoFrame
                {
                    Codec = frameIn.Codec,
                    Height = frameIn.Height,
                    Width = frameIn.Width,
                    Stream = frameStream
                };

                if (frame == null)
                    throw new Exception("Trying to use wrong frame type in ImageDecode");

                if (frame.Stream == null)
                    throw new Exception("Frame passed with empty stream");
                
                logger.Trace($"Decoding new frame of size {frame.Stream.Length}");
                
                if(!File.Exists("out.h264"))
                    File.WriteAllBytes("out.h264",frame.Stream.ToArray());

                var inStream = frame.Stream;
                
                if(!ffmpegInitialised)
                    InitialiseFFMpeg(ref inStream);
                else
                    avioReader!.RefreshInputStream(inStream);
                
                var convertedFrame = vfc!.Convert(vsd!.DecodeNextFrame());
                var convertedFrameSize = convertedFrame.height * convertedFrame.linesize[0];
                
                uint idx = 0;
                for (uint i = 0; i < 8; i++)
                {
                    if (convertedFrame.data[i] != null)
                    {
                        idx = i;
                        break;
                    }
                }
                
                return new UnsafeUnmanagedVideoFrame()
                {
                    Width = width,
                    Height = height,
                    FrameData = convertedFrame.data[idx],
                    FrameSize = convertedFrameSize
                };
            }
        }
        catch (SocketException e)
        {
            logger.Error(e);
        }

        return new UnsafeUnmanagedVideoFrame();
    }
}
