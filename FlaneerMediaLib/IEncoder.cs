using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
{
    public interface IEncoder : IService
    {
        VideoFrame GetFrame();
    }
}
