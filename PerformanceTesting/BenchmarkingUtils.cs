namespace PerformanceTesting;

public static class BenchmarkingUtils
{
    private const string Path = "C:/Users/Tom/Code/FlaneerStreamingApp/TestResources/SamplePackets/";

    public static string GetPacket(int i) => $"{Path}packet{i}.bin";
}
