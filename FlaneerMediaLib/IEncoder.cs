namespace FlaneerMediaLib
{
    public interface IEncoder : IService
    {
        VideoFrame GetFrame();
    }
}
