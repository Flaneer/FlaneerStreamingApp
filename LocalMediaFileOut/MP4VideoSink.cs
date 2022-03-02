using FlaneerMediaLib;

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

        public void Capture(int numberOfFrames)
        {
            for (int i = 0; i < numberOfFrames; i++)
            {
                try
                {
                    var ptrToFrame = encoder.GetFrame();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                
            }
        }
    }
}
