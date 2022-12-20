using FFmpeg.AutoGen;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoStreaming;
using FlaneerMediaLib.VideoStreaming.ffmpeg;
using OfflinePacketSimulator;
using Xunit;

namespace MediaLibTests;

public class FFMpegTests
{
    private OfflinePacketBuffer offlinePacketBuffer = new OfflinePacketBuffer();
    private FrameBuffer  frameBuffer => offlinePacketBuffer.FrameBuffer;
    
    private AVIOReader avioContext;
    private VideoStreamDecoder videoStreamDecoder;

    public FFMpegTests()
    {
        FFmpegLauncher.InitialiseFFMpeg();
        
        var args = new []{"-framesize","1280","720"};
        CommandLineArgumentStore.CreateAndRegister(args);
        
        var frameStream = offlinePacketBuffer.GetFirstFrameStream();
        avioContext = new AVIOReader(frameStream);
        videoStreamDecoder = new VideoStreamDecoder();
    }

    [Fact]
    public void TestStreamDecoderInit()
    {
        unsafe
        {
            videoStreamDecoder.Init(avioContext.AvioCtx);
            
            Assert.True(videoStreamDecoder.CodecContextPtr->width == 1280);
            Assert.True(videoStreamDecoder.CodecContextPtr->height == 720);
            
        }
    }
}
