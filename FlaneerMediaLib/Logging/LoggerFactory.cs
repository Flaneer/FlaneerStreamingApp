using Spectre.Console;

namespace FlaneerMediaLib.Logging;

/// <summary>
/// Can be used to create and track loggers
/// </summary>
internal class LoggerFactory
{
    private static LoggerFactory instance = null!;

    // ReSharper disable once ConstantNullCoalescingCondition
    private static LoggerFactory Instance => instance ??= new LoggerFactory();

    private List<string> log = new();

    public static Logger CreateLogger(object obj) => new Logger(obj.GetType(), Instance);

    /// <summary>
    /// 
    /// </summary>
    public void Info(string s, string typeString)
    {
        var message = $"[bold green]<INFO :{typeString}>[/] {s}";
        AnsiConsole.MarkupLine(message);
        log.Add(message);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Debug(string s, string typeString)
    {
        var message = $"[bold yellow]<DEBUG:{typeString}>[/] {s}";
        AnsiConsole.MarkupLine(message);
        log.Add(message);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Error(Exception exception, string typeString)
    {
        var message = $"[bold red]<ERROR:{typeString}>[/] {exception}";
        AnsiConsole.MarkupLine(message);
        log.Add(message);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Error(string exception, string typeString)
    {
        var message = $"[bold red]<ERROR:{typeString}>[/] {exception}";
        AnsiConsole.MarkupLine(message);
        log.Add(message);
    }
}