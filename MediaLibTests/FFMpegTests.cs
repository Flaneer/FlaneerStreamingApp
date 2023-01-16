using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;
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

    private static readonly string[] INPUT = new[] {"-framesize", "1280", "720", "-nonet"};
    
    public FFMpegTests()
    {
        CommandLineArgumentStore.CreateAndRegister(INPUT);
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        
        FFmpegLauncher.InitialiseFFMpeg();
        
        var frameStream = offlinePacketBuffer.GetFirstFrameStream();
        avioContext = new AVIOReader(frameStream);
        videoStreamDecoder = new VideoStreamDecoder();
    }

    //It would be better to have this split into two tests, but I'm not sure how to do that with xUnit
    [Fact (Skip = "Not working on CI")]
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

    [Fact (Skip = "Not a test, just useful code")]
    public void TestCannedH264ToJpeg()
    {
        var w = 1280;
        var h = 720;
        
        avioContext = new AVIOReader(offlinePacketBuffer.GetFirstFrameStream());
        DecodeFrames(w, h);
    }
    
    [Fact]
    public unsafe void TestLiveH264ToJpeg()
    {
        var w = 1280;
        var h = 720;

        InitialiseMediaEncoder();
        ServiceRegistry.TryGetService<IEncoder>(out var encoder);

        var seedFrame = encoder.GetFrame() as UnmanagedVideoFrame;
        byte[] seedFrameBytes = new byte[seedFrame.FrameSize];
        Marshal.Copy(seedFrame.FrameData, seedFrameBytes, 0, seedFrame.FrameSize);

        var seedStream = new MemoryStream(seedFrame.FrameSize);
        seedStream.Write(seedFrameBytes, 0, seedFrame.FrameSize);
        
        avioContext = new AVIOReader(seedStream);
        videoStreamDecoder.Init(avioContext.AvioCtx, w, h);

        for (int frameNum = 0; frameNum < 100; frameNum++)
        {
            var destinationSize = videoStreamDecoder.SourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
            var vfc = new VideoFrameConverter(videoStreamDecoder.SourceSize, videoStreamDecoder.SourcePixelFormat,
                destinationSize, destinationPixelFormat);

            var frame = videoStreamDecoder.DecodeNextFrame();
            var convertedFrame = vfc.Convert(frame);

            var convertedFrameSize = convertedFrame.height * convertedFrame.linesize[0];

            byte[] bytes = new byte[convertedFrameSize];
            Marshal.Copy((IntPtr) convertedFrame.data[0], bytes, 0, convertedFrameSize);

            SaveFrameToJpeg(w, h, bytes, frameNum);

            var nextFrame = encoder.GetFrame() as UnmanagedVideoFrame;
            byte[] nextFrameBytes = new byte[nextFrame.FrameSize];
            Marshal.Copy(nextFrame.FrameData, nextFrameBytes, 0, nextFrame.FrameSize);

            var nextStream = new MemoryStream(nextFrame.FrameSize);
            nextStream.Write(nextFrameBytes, 0, nextFrame.FrameSize);
            avioContext.RefreshInputStream(nextStream);
        }
    }

    private static void InitialiseMediaEncoder()
    {
        var videoSettings = new VideoSettings();
        var frameSettings = new FrameSettings()
        {
            Height = (short) videoSettings.Height,
            Width = (short) videoSettings.Width,
            MaxFPS = videoSettings.MaxFPS
        };

        var codecSettings = new H264CodecSettings()
        {
            Format = videoSettings.Format,
            GoPLength = (short)videoSettings.GoPLength
        };
        
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.NvEncH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
    
    private unsafe void DecodeFrames(int w, int h)
    {
        videoStreamDecoder.Init(avioContext.AvioCtx, w, h);


        for (int frameNum = 0; frameNum < 100; frameNum++)
        {
            var destinationSize = videoStreamDecoder.SourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
            var vfc = new VideoFrameConverter(videoStreamDecoder.SourceSize, videoStreamDecoder.SourcePixelFormat,
                destinationSize, destinationPixelFormat);

            var frame = videoStreamDecoder.DecodeNextFrame();
            var convertedFrame = vfc.Convert(frame);

            var convertedFrameSize = convertedFrame.height * convertedFrame.linesize[0];

            byte[] bytes = new byte[convertedFrameSize];
            Marshal.Copy((IntPtr) convertedFrame.data[0], bytes, 0, convertedFrameSize);

            SaveFrameToJpeg(w, h, bytes, frameNum);

            avioContext.RefreshInputStream(offlinePacketBuffer.GetFrame(frameNum + 1));
        }
    }
    
    private static void SaveFrameToJpeg(int w, int h, byte[] bytes, int frameNum)
    {
        int bpp = 24;
        Bitmap bmp = new Bitmap(w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = ((y * w) + x) * (bpp / 8);
                if (bpp == 24) // in this case you have 3 color values (red, green, blue)
                {
                    // first byte will be red, because you are writing it as first value
                    byte r = bytes[i];
                    byte g = bytes[i + 1];
                    byte b = bytes[i + 2];
                    Color color = Color.FromArgb(r, g, b);
                    bmp.SetPixel(x, y, color);
                }
            }
        }

        bmp.Save($"test-{frameNum}.jpg");
    }
}
