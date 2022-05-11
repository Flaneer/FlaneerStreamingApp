using System.Diagnostics;

namespace FlaneerMediaLib;

public class FFMpegDecoder : IDisposable
{
    public const string FFMPEGVERSION  = "4.4.1";
    public const string FFMPEGPATH = "ffmpeg/ffmpeg.exe";
    
    private Process ffmpegProcess = new Process();
    private MemoryStream frameOut;

    public FFMpegDecoder(string logLevelIn = "quiet")
    {
        ffmpegProcess.StartInfo.FileName = FFMPEGPATH;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardInput = true;
        
        var logLevel = $"-loglevel {logLevelIn}";
        var inputFormat = "-f h264";
        var input = "-i pipe:0";
        var pixFmt = "-pix_fmt rgba";
        var codec = "-c:v rawvideo";
        var numFrames = "-vframes 1";
        var outFormat = "-f rawvideo";
        var output = "pipe:1";
        ffmpegProcess.StartInfo.Arguments = $"{logLevel} -re {inputFormat} {input} {pixFmt} {codec} {numFrames} {outFormat} {output}";

        //TODO: pass as params
        frameOut = new MemoryStream(1920*1080*32);
    }

    /// <summary>
    /// Decodes a new frame
    /// </summary>
    public MemoryStream DecodeFrame(MemoryStream encodedFrame)
    {
        frameOut.Position = 0;
        
        ffmpegProcess.Start();

        ffmpegProcess.StandardInput.BaseStream.Write(encodedFrame.ToArray());
        ffmpegProcess.StandardInput.Close();

        ffmpegProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"{args.Data}");
        
        ffmpegProcess.StandardOutput.BaseStream.CopyTo(frameOut);
            
        ffmpegProcess.WaitForExit();

        return frameOut;
    }

    public void Dispose()
    {
        ffmpegProcess.Dispose();
        frameOut.Dispose();
    }
}
