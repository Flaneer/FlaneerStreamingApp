using System.Runtime.InteropServices;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Loads the ffmpeg binaries
    /// </summary>
    public class FFmpegBinariesHelper
    {
        /// <summary>
        /// Loads the ffmpeg binaries
        /// </summary>
        public static void RegisterFFmpegBinaries()
        {
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var current = Environment.CurrentDirectory;
                var probe = Path.Combine(current, "ffmpeg");

                while (current != null)
                {
                    var ffmpegBinaryPath = Path.Combine(current, probe);

                    if (Directory.Exists(ffmpegBinaryPath))
                    {
                        Console.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                        FFmpeg.AutoGen.ffmpeg.RootPath = ffmpegBinaryPath;
                        return;
                    }

                    current = Directory.GetParent(current)?.FullName;
                }
            }
            else
                throw new NotSupportedException();
        }
    }
}
