using System;
using System.Diagnostics;
using System.IO;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class FFMpegTests
{
    

    [Fact]
    public void TestFFMpegIsPresent()
    {
        Assert.True(File.Exists(FFMpegDecoder.FFMPEGPATH));
    }

    [Fact]
    public void TestFFMpegVersionIsCorrect()
    {
        Process ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = FFMpegDecoder.FFMPEGPATH;
        ffmpegProcess.StartInfo.Arguments = $"-version";
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        
        ffmpegProcess.Start();

        string output = ffmpegProcess.StandardOutput.ReadLine();
        ffmpegProcess.WaitForExit();
        
        Assert.Contains($"version {FFMpegDecoder.FFMPEGVERSION}", output);
    }

    [Fact]
    public void TestSampleFileConversion()
    {
        var outputPath = "testOutput.mp4";
        if(File.Exists(outputPath))
            File.Delete(outputPath);

        Process ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = FFMpegDecoder.FFMPEGPATH;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;

        var testFilePath = "TestResources/sunflower_1080p25-0.webm";
        ffmpegProcess.StartInfo.Arguments = $"-i {testFilePath} {outputPath}";

        ffmpegProcess.Start();
        
        ffmpegProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"{args.Data}");
        
        ffmpegProcess.WaitForExit();

        Assert.True(File.Exists(outputPath));
        
        var fileInfo = new FileInfo(outputPath);
        Assert.Equal(7432921,fileInfo.Length);
    }
}