using System.IO;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

[Collection("Sequential")]
public class CommandLineArgumentStoreTests
{
    private static readonly string[] INPUT = new[] {"-arg1", "-arg2", "param", "-arg3", "param1", "param2", "param3"};
    
    [Fact]
    public void TestParamParse()
    {
        ServiceRegistry.ClearRegistry();
        CommandLineArgumentStore.CreateAndRegister(INPUT);
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        
        Assert.True(clArgStore.HasArgument("arg1"));
        
        var arg1Params = clArgStore.GetParams("arg1");
        Assert.Equal(new string[]{}, arg1Params);
        
        var arg2Params = clArgStore.GetParams("arg2");
        Assert.Equal(new string[]{"param"}, arg2Params);
        
        var arg3Params = clArgStore.GetParams("arg3");
        Assert.Equal(new string[]{"param1", "param2", "param3"}, arg3Params);
    }

    [Fact]
    public void TestParseFromFile()
    {
        var rawString = "[Streaming]\narg1 = 1234\narg2 = 5678 9012\n-arg3 = 3456";
        File.WriteAllText("FlaneerStreamingConfig.ini", rawString);
        
        ServiceRegistry.ClearRegistry();
        CommandLineArgumentStore.CreateAndRegister(new []{"-arg3", "777"});
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        
        
        //Test single param
        Assert.True(clArgStore.HasArgument("arg1"));
        
        var arg1Params = clArgStore.GetParams("arg1");
        Assert.Equal(new[]{"1234"}, arg1Params);
        
        //Test double param
        Assert.True(clArgStore.HasArgument("arg2"));
        
        var arg2Params = clArgStore.GetParams("arg2");
        Assert.Equal(new[]{"5678", "9012"}, arg2Params);
        
        //Test override
        Assert.True(clArgStore.HasArgument("arg3"));
        
        var arg3Params = clArgStore.GetParams("arg3");
        Assert.Equal(new[]{"777"}, arg3Params);
    }
}
