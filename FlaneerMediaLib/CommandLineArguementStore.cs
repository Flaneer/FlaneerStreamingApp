namespace FlaneerMediaLib;

/// <summary>
/// Class that parses CL arguments and stores them in a dictionary
/// </summary>
public class CommandLineArguementStore : IService
{
    Dictionary<string, List<string>> arguments = new();

    /// <summary>
    /// ctor
    /// </summary>
    public CommandLineArguementStore()
    {
    }

    /// <summary>
    /// Parse command line arguments and put them in the store
    /// </summary>
    public void ParseArguements(string[] commandLineArgs)
    {
        var lastArg = "";
        foreach (var arg in commandLineArgs)
        {
            if (arg.First() == '-')
            {
                lastArg = arg.Substring(1);
                arguments.Add(lastArg, new List<string>());
            }
            else
            {
                arguments[lastArg].Add(arg);
            }
        }
    }

    /// <summary>
    /// Check if an argument has been provided
    /// </summary>
    public bool HasArgument(string arg) => arguments.ContainsKey(arg);

    /// <summary>
    /// Returns the parameters of an argument
    /// </summary>
    public string[] GetParams(string arg) => arguments.ContainsKey(arg) ? arguments[arg].ToArray() : new string[]{};

    /// <summary>
    /// Creates a new arg store 
    /// </summary>
    public static void CreateAndRegister(string[] commandLineArgs)
    {
        var clas = new CommandLineArguementStore();
        clas.ParseArguements(commandLineArgs);
        ServiceRegistry.AddService(clas);
    }
}
