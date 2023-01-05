namespace OfflinePacketSimulator;

public static class OfflinePacketAccess
{
    private static string workingDirectory = Environment.CurrentDirectory;
    private static string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
    private static string path = Path.Combine(projectDirectory, "TestResources\\PretransSamplePackets");

    public static string GetPacket(int i) => Path.Combine(path, $"packet{i}.bin");
}
