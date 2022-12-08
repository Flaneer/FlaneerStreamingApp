using Microsoft.IO;

namespace FlaneerMediaLib.SmartStorage;

internal class SmartMemoryStreamManager : IService
{
    private readonly RecyclableMemoryStreamManager manager = new ();

    public MemoryStream GetStream() => manager.GetStream();
    public MemoryStream GetStream(int size) => manager.GetStream(Guid.NewGuid().ToString(), size);
    public MemoryStream GetStream(byte[] buffer) => manager.GetStream(buffer);
    public MemoryStream GetStream(byte[] buffer, int index, int bufferSize) => manager.GetStream(Guid.NewGuid().ToString(), buffer, index, bufferSize);
}
