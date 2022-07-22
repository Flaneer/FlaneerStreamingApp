using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FlaneerMediaLib.Logging;

using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Loads the ffmpeg binaries
    /// </summary>
    public class FFmpegLauncher
    {
        private Logger logger;
        
        private static FFmpegLauncher instance = null!;

        private FFmpegLauncher()
        {
        }

        // ReSharper disable once ConstantNullCoalescingCondition
        private static FFmpegLauncher Instance => instance ??= new FFmpegLauncher();
        
        /// <summary>
        /// Start ffmpeg
        /// </summary>
        public static void InitialiseFFMpeg()
        {
            RegisterBinaries();
            SetupLogging();
        }

        private static unsafe void SetupLogging()
        {
            FF.av_log_set_level(FF.AV_LOG_VERBOSE);

            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > FF.av_log_get_level()) return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                FF.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr) lineBuffer);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(line);
                Console.ResetColor();
            };

            FF.av_log_set_callback(logCallback);
        }
        
        /// <summary>
        /// TODO: Make this work safely, it is a nice to have so not spending any more time now
        /// </summary>
        /// <param name="avcl">A pointer to an arbitrary struct of which the first field is a pointer to an AVClass struct.</param>
        /// <param name="level">level	The importance level of the message expressed using a Logging Constant. </param>
        /// <param name="fmt">The format string (printf-compatible) that specifies how subsequent arguments are converted to output. </param>
        /// <param name="vl">The arguments referenced by the format string. </param>
        private static unsafe void FlaneerLogCallback(void* avcl, int level, [MarshalAs((UnmanagedType) 0)] string fmt, byte* vl)
        {
            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            FF.av_log_format_line(avcl, level, fmt, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr) lineBuffer);
            line ??= "";
        
            switch (level)
            {
                case FF.AV_LOG_DEBUG:
                    Instance.logger.Debug(line);
                    break;
                case FF.AV_LOG_ERROR:
                case FF.AV_LOG_FATAL:
                case FF.AV_LOG_PANIC:
                case FF.AV_LOG_WARNING:
                    Instance.logger.Error(line);
                    break;
                case FF.AV_LOG_INFO:
                    Instance.logger.Info(line);
                    break;
                case FF.AV_LOG_QUIET:
                case FF.AV_LOG_TRACE:
                case FF.AV_LOG_VERBOSE:
                    Instance.logger.Trace(line);
                    break;
                default:
                    Instance.logger.Trace(line);
                    break;
            }
            
        }

        private static void RegisterBinaries()
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
                        FF.RootPath = ffmpegBinaryPath;
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
