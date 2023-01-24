using Salaros.Configuration;

namespace FlaneerMediaLib;

/// <summary>
/// Class that parses CL arguments and stores them in a dictionary
/// </summary>
public class CommandLineArgumentStore : IService
{
    private readonly Dictionary<string, List<string>> arguments = new();

    internal CommandLineArgumentStore(){}
    
    /// <summary>
    /// Parse command line arguments and put them in the store
    /// </summary>
    public void ParseArguments(string[] commandLineArgs)
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
        var clas = new CommandLineArgumentStore();
        clas.ParseArguments(commandLineArgs);
        clas.LoadFromConfigFile();
        ServiceRegistry.AddService(clas);
    }

    //TODO: Add support for args with no params, there is no obvious way to do this with the current implementation, nor an obvious need to do it
    private void LoadFromConfigFile()
    {
        var flaneerStreamingConfigPath = "FlaneerStreamingConfig.ini";
        var ret = new string[] { };
        if(File.Exists(flaneerStreamingConfigPath))
        {
            var configFile = new ConfigParser(flaneerStreamingConfigPath);
            foreach (var configFileSection in configFile.Sections)
            {
                foreach (var configKeyValue in configFileSection.Keys)
                {
                    if(!HasArgument(configKeyValue.Name))
                    {
                        var rawValue = configKeyValue.Content;
                        var values = rawValue.Split(' ');
                        arguments.Add(configKeyValue.Name, values.ToList());
                    }
                }
            }
        }
    }
}
