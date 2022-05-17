using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class CommandLineArgumentStoreTests
{
    private static readonly string[] INPUT = new[] {"-arg1 -arg2 param -arg3 param1 param2 param3"};
    
    [Fact]
    public void TestArgParse()
    {
        CommandLineArguementStore.CreateAndRegister(INPUT);
        ServiceRegistry.TryGetService<CommandLineArguementStore>(out var clas);

        clas.GetParams("arg1");
    }
}