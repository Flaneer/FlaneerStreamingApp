using Microsoft.IO;

namespace FlaneerMediaLib;

internal class SmartMemoryStreamManager : IService
{
    private readonly RecyclableMemoryStreamManager manager = new ();

    public MemoryStream GetStream => manager.GetStream();
}
