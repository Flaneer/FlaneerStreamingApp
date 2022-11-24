using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace PerformanceTesting;

public class Program
{
    private static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        //var summary = BenchmarkRunner.Run(typeof(UDPReceptionBenchmarking));
    }
}
