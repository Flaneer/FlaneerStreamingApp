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
    public void ParseArguements(string commandLineArgs)
    {
        var args = commandLineArgs.Split("-");
        foreach (var arg in args)
        {
            var splitArgs = arg.Split();
            if (splitArgs.Length == 0)
            {
                continue;
            }
            if(splitArgs.Length > 0)
            {
                arguments.Add(splitArgs[0], new List<string>());
            }
            if (splitArgs.Length > 1)
            {
                for (int i = 1; i < splitArgs.Length; i++)
                {
                    arguments[splitArgs[0]].Add(splitArgs[i]);
                }
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
    public static void CreateAndRegister(string commandLineArgs)
    {
        var clas = new CommandLineArguementStore();
        clas.ParseArguements(commandLineArgs);
        ServiceRegistry.AddService(clas);
    }
}
