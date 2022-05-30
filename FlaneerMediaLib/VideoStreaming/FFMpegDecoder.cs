using System.Diagnostics;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib;

/// <summary>
/// Media decoder using ffmpeg
/// </summary>
public class FFMpegDecoder : IDisposable
{
    /// <summary>
    /// The currently used version of ffmpeg
    /// </summary>
    public const string FFMPEGVERSION  = "4.4.1";
    /// <summary>
    /// The location of the ffmpeg exe in the bin folder
    /// </summary>
    public const string FFMPEGPATH = "ffmpeg/ffmpeg.exe";
    
    private Process ffmpegProcess = new Process();
    private MemoryStream frameOut;
    
    private Logger logger;
    private readonly int width;
    private readonly int height;

    /// <summary>
    /// ctor
    /// </summary>
    public FFMpegDecoder(string logLevelIn = "quiet")
    {
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.FrameSettings);
        width = Int32.Parse(frameSettings[0]);
        height = Int32.Parse(frameSettings[1]);
        
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
        frameOut = new MemoryStream(width*height*32);
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

        ffmpegProcess.OutputDataReceived += (sender, args) => logger.Info($"{args.Data}");
        
        ffmpegProcess.StandardOutput.BaseStream.CopyTo(frameOut);
            
        ffmpegProcess.WaitForExit();

        return frameOut;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ffmpegProcess.Dispose();
        frameOut.Dispose();
    }
}
