namespace FlaneerMediaLib.SmartStorage;

/// <summary>
/// Static class that can be used in the main to start all the smart storage objects and their managers
/// </summary>
public static class SmartStorageSubsystem
{
    /// <summary>
    /// Initialises the smart storage objects and their managers
    /// </summary>
    public static void InitSmartStorage()
    {
        var smartBuffer = new SmartBufferManager();
        ServiceRegistry.AddService(smartBuffer);

        var smartMemory = new SmartMemoryStreamManager();
        ServiceRegistry.AddService(smartMemory);
    }
}
