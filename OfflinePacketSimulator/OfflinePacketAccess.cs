namespace OfflinePacketSimulator;

public static class OfflinePacketAccess
{
    private static string workingDirectory = Environment.CurrentDirectory;
    private static string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
    private static string path = "C:\\Users\\Tom\\Code\\FlaneerStreamingApp\\TestResources\\SamplePackets";//Path.Combine(projectDirectory, "TestResources\\SamplePackets");

    public static string GetPacket(int i) => Path.Combine(path, $"packet{i}.bin");
}
