using Microsoft.IO;

namespace FlaneerMediaLib.SmartStorage;

internal class SmartMemoryStreamManager : IService
{
    private readonly RecyclableMemoryStreamManager manager = new ();

    public MemoryStream GetStream => manager.GetStream();
}
