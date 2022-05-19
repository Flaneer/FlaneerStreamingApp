using Spectre.Console;

namespace FlaneerMediaLib;

/// <summary>
/// 
/// </summary>
public class Logger
{
    private Type loggerType;
    private string typeString;

    private Logger(Type loggerType)
    {
        this.loggerType = loggerType;
        typeString = loggerType.ToString().Split('.').Last();
    }

    /// <summary>
    /// 
    /// </summary>
    public static Logger GetLogger(object obj)
    {
        return new Logger(obj.GetType());
    }

    /// <summary>
    /// 
    /// </summary>
    public void Info(string s)
    {
        AnsiConsole.Markup($"[bold green](INFO :{typeString})[/] {s}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Debug(string s)
    {
        AnsiConsole.Markup($"[bold yellow](DEBUG:{typeString})[/] {s}\n");
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Error(Exception exception)
    {
        AnsiConsole.Markup($"[bold red](ERROR:{typeString})[/] {exception}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Error(string exception)
    {
        AnsiConsole.Markup($"[bold red](ERROR:{typeString})[/] {exception}\n");
    }
}