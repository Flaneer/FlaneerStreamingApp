using FlaneerMediaLib;
using System.Diagnostics;

namespace LocalMediaFileOut
{
    internal class MP4VideoSink : IVideoSink
    {
        IEncoder encoder;

        public MP4VideoSink()
        {
            if(ServiceRegistry.TryGetService<IEncoder>(out var encoder))
                this.encoder = encoder;
            else
                throw new Exception("No available encoder");
        }

        public unsafe void Capture(int numberOfFrames, int targetFramerate)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var frameLength = new TimeSpan(0, 0, (int)Math.Floor(1.0f/ targetFramerate));

            FileStream file;
            using (file = new FileStream("out.h264", FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < numberOfFrames; i++)
                {

                    while (stopWatch.Elapsed < (frameLength*i))
                    {
                        Thread.Sleep(1);
                    }

                    try
                    {
                        var frame = encoder.GetFrame();
                        using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*)frame.FrameData, frame.FrameSize))
                        {
                            ustream.CopyTo(file);
                        }   
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
                stopWatch.Stop();
            }
        }
    }
}
