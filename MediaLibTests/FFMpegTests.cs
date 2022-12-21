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
        
        var frameStream = offlinePacketBuffer.GetFirstFrameStream();
        avioContext = new AVIOReader(frameStream);
        videoStreamDecoder = new VideoStreamDecoder();
    }

    //It would be better to have this split into two tests, but I'm not sure how to do that with xUnit
    [Fact]
    public void TestStreamDecoder()
    {
        unsafe
        {
            var w = 1280;
            var h = 720;
            videoStreamDecoder.Init(avioContext.AvioCtx, w, h);
            
            Assert.True(videoStreamDecoder.CodecContextPtr != null);

            Assert.Equal(w, videoStreamDecoder.CodecContextPtr->width);
            Assert.Equal(h, videoStreamDecoder.CodecContextPtr->height);
            
            var frame = videoStreamDecoder.DecodeNextFrame();

            var frameInfo = TestUtils.GetFrameInfo(frame);
            Assert.Equal(0, frameInfo.CodedPictureNumber);
            //This is the first frame, so it should be a key frame
            Assert.True(frameInfo.KeyFrame);
            Assert.Equal(AVPixelFormat.AV_PIX_FMT_NV12, frameInfo.Format);
        }
    }
}
