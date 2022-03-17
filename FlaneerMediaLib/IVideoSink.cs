namespace FlaneerMediaLib
{
    public interface IVideoSink
    {
        void CaptureFrame();
        void CaptureFrames(int numberOfFrames, int targetFramerate);
    }
}
