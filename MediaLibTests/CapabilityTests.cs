using System.IO;
using Xunit;

namespace MediaLibTests;

public class CapabilityTests
{
    [Fact]
    public void TestIfICanWriteAFile()
    {
        var outputPath = "testfile.file";
        File.Create(outputPath);
        Assert.True(File.Exists(outputPath));
    }
}