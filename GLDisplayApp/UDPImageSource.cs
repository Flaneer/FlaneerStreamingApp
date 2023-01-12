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

    private int decodeCount;
    private TimeSpan totalDecodeTime;

    private bool alsoWriteToFile = false;

    public UDPImageSource()
    {
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        var frameSettings = clArgStore.GetParams(CommandLineArgs.FrameSettings);
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
                
                var frame = frameIn as ManagedVideoFrame;

                if (frame == null)
                    throw new Exception("Trying to use wrong frame type in ImageDecode");

                if (frame.Stream == null)
                    throw new Exception("Frame passed with empty stream");
                
                logger.Trace($"Decoding new frame of size {frame.Stream.Length}");
                
                WriteToFile(frame);

                var inStream = frame.Stream;
                
                if(!ffmpegInitialised)
                    InitialiseFFMpeg(ref inStream);
                else
                    avioReader!.RefreshInputStream(inStream);
                
                var decodeStartTime = DateTime.Now;
                var convertedFrame = vfc!.Convert(vsd!.DecodeNextFrame());
                var decodeTime = DateTime.Now - decodeStartTime;
                logger.TimeStat("Decode", decodeTime);

                totalDecodeTime += decodeTime;
                decodeCount++;
                
                logger.TimeStat("Average Decode", totalDecodeTime/decodeCount);
                
                var convertedFrameSize = convertedFrame.height * convertedFrame.linesize[0];
                return new UnsafeUnmanagedVideoFrame()
                {
                    Width = width,
                    Height = height,
                    FrameData = convertedFrame.data[0],
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

    private void WriteToFile(ManagedVideoFrame frame)
    {
        if (alsoWriteToFile)
        {
            var frameBytes = frame.Stream.ToArray();
            try
            {
                var f = File.Open("out.h264", FileMode.Append);
                f.Write(frameBytes);
                f.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
