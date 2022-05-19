using System.Diagnostics;
using System.Reflection;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace GLFWTestApp;

internal static class Program
{
    static string NameOfCallingClass()
    {
        string fullName;
        Type declaringType;
        int skipFrames = 2;
        do
        {
            MethodBase method = new StackFrame(skipFrames, false).GetMethod();
            declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return method.Name;
            }
            skipFrames++;
            fullName = declaringType.FullName;
        }
        while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

        return fullName;
    }
    
    private static void Main(string[] args)
    {
        var x = NameOfCallingClass();
        
        InitialiseMediaEncoder();
        GLWindow window = new GLWindow(1920, 1080);
        GLEnv env = new GLEnv(window);
        window.StartAppLoop();
    }

    class VideoSettings
    {
        public int Height = 1080;
        public int Width = 1920;
        public int MaxFPS = 60;
        public BufferFormat Format = BufferFormat.ARGB;
        public int GoPLength = 5;
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
        MediaEncoderLifeCycleManager encoderLifeCycleManager = new MediaEncoderLifeCycleManager(VideoSource.UDPH264);
        encoderLifeCycleManager.InitVideoSource(frameSettings, codecSettings);
    }
}