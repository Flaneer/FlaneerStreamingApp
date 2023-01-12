using System.Net.Sockets;
using FFmpeg.AutoGen;
using FFmpegDecoderWrapper;
using FlaneerMediaLib;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;
using FlaneerMediaLib.VideoStreaming.ffmpeg;
using AVPixelFormat = FFmpegDecoderWrapper.AVPixelFormat;

namespace GLDisplayApp;

public class UDPImageSourceNative
{
    private readonly IVideoSource videoSource;
    
    private readonly Logger logger;
    
    private readonly short width;
    private readonly short height;

    private int decodeCount;
    private TimeSpan totalDecodeTime;

    private bool alsoWriteToFile = false;

    private bool ffmpegInitialised = false;

    public UDPImageSourceNative()
    {
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        var frameSettings = clArgStore.GetParams(CommandLineArgs.FrameSettings);
        width =  Int16.Parse(frameSettings[0]);
        height = Int16.Parse(frameSettings[1]);

        ServiceRegistry.TryGetService(out videoSource);
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
                    throw new Exception($"Trying to use {frameIn.GetType()} instead of {typeof(ManagedVideoFrame)} in ImageDecode");

                if (frame.Stream == null)
                    throw new Exception("Frame passed with empty stream");
                
                logger.Trace($"Decoding new frame of size {frame.Stream.Length}");
                
                WriteToFile(frame.Stream.ToArray());
                
                var decodeStartTime = DateTime.Now;
                FrameRequest newFrame;
                fixed (void* p = frame.Stream.GetBuffer())
                {
                    if(ffmpegInitialised)
                    {
                        newFrame = Wrapper.RequestNewFrame((IntPtr) p, (int) frame.Stream.Length, width, height);
                    }
                    else
                    {
                        var videoFrameSettings = new VideoFrameSettings
                        {
                            Height = height,
                            Width = width,
                            Codec = Codec.H264,
                            PixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P
                        };
                        newFrame = Wrapper.Init(videoFrameSettings, (IntPtr)p, (int)frame.Stream.Length);
                        ffmpegInitialised = true;
                    }
                }

                var decodeTime = DateTime.Now - decodeStartTime;
                logger.TimeStat("Decode", decodeTime);

                totalDecodeTime += decodeTime;
                decodeCount++;
                
                logger.TimeStat("Average Decode", totalDecodeTime/decodeCount);
                
                return new UnsafeUnmanagedVideoFrame()
                {
                    Width = width,
                    Height = height,
                    FrameData = (byte*) newFrame.DataOut,
                    FrameSize = newFrame.BufferSizeOut
                };
            }
        }
        catch (SocketException e)
        {
            logger.Error(e);
        }

        return new UnsafeUnmanagedVideoFrame();
    }

    private void WriteToFile(byte[] frameBytes)
    {
        if (alsoWriteToFile)
        {
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
