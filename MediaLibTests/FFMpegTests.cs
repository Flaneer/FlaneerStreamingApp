using System;
using FFmpeg.AutoGen;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoStreaming;
using FlaneerMediaLib.VideoStreaming.ffmpeg;
using OfflinePacketSimulator;
using Xunit;
using FF = FFmpeg.AutoGen.ffmpeg;

namespace MediaLibTests;

[Collection("Sequential")]
public class FFMpegTests
{
    private OfflinePacketBuffer offlinePacketBuffer = new OfflinePacketBuffer();
    private FrameBuffer  frameBuffer => offlinePacketBuffer.FrameBuffer;

    private AVIOReader avioContext;
    private VideoStreamDecoder videoStreamDecoder;

    public FFMpegTests()
    {
        FFmpegLauncher.InitialiseFFMpeg();

        var w = "1280";
        var h = "720";
        var args = new []{"-framesize",w,h};
        CommandLineArgumentStore.CreateAndRegister(args);
        
        var frameStream = offlinePacketBuffer.GetFirstFrameStream();
        avioContext = new AVIOReader(frameStream);
        videoStreamDecoder = new VideoStreamDecoder();
    }

    [Fact]
    public void TestStreamDecoder()
    {
        unsafe
        {
            videoStreamDecoder.Init(avioContext.AvioCtx);
            
            Assert.True(videoStreamDecoder.CodecContextPtr != null);
            
            Assert.True(videoStreamDecoder.CodecContextPtr->width == 1280);
            Assert.True(videoStreamDecoder.CodecContextPtr->height == 720);
            
            var frame = videoStreamDecoder.DecodeNextFrame();

            var frameInfo = VideoStreamDecoder.GetFrameInfo(frame);
            
            Assert.Equal(0, frameInfo.CodedPictureNumber);

            //This is the first frame, so it should be a key frame
            Assert.True(frameInfo.KeyFrame);
            
            Assert.Equal(AVPixelFormat.AV_PIX_FMT_NV12, frameInfo.Format);
        }
    }
}
