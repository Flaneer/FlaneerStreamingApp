using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

[CollectionDefinition("Sequential", DisableParallelization = true)]
[Collection("Sequential")]
public class CommandLineArgumentStoreTests
{
    private static readonly string[] INPUT = new[] {"-arg1", "-arg2", "param", "-arg3", "param1", "param2", "param3"};
    
    [Fact]
    public void TestArgParse()
    {
        CommandLineArguementStore.CreateAndRegister(INPUT);
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);

        Assert.True(clas.HasArgument("arg1"));
    }
    
    [Fact]
    public void TestParamParse()
    {
        CommandLineArguementStore.CreateAndRegister(INPUT);
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);

        var arg1Params = clas.GetParams("arg1");
        Assert.Equal(new string[]{}, arg1Params);
        
        var arg2Params = clas.GetParams("arg2");
        Assert.Equal(new string[]{"param"}, arg2Params);
        
        var arg3Params = clas.GetParams("arg3");
        Assert.Equal(new string[]{"param1", "param2", "param3"}, arg3Params);
    }
}