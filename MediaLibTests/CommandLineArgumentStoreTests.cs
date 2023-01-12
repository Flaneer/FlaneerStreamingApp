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
}
