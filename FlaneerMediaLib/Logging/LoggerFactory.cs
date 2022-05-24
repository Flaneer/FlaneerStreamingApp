using System.Diagnostics;
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
    
    private string TimeString() => $"[gray]<{DateTime.Now.ToString("HH:mm:ss")}>[/]";

    private string GetLogPrefix(string formatting, string typeString)
    {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(3);
        var method = $"{sf.GetMethod().Name}";
        var fileLineNumber = $"{sf.GetFileLineNumber()}";

        return $"[{formatting}]<{typeString}:{fileLineNumber}[/][gray]({method})[/][{formatting}]>[/]";
    }
    
    internal static Logger CreateLogger(object obj) => new Logger(obj.GetType(), Instance);

    internal void Info(string s, string typeString)
    {
        var message = $"{TimeString()}{GetLogPrefix("bold green", typeString)} {s}";
        AnsiConsole.MarkupLine(message);
    }
    
    internal void Debug(string s, string typeString)
    {
        var message = $"{TimeString()}{GetLogPrefix("bold yellow", typeString)} {s}";
        AnsiConsole.MarkupLine(message);
    }
    
    internal void Error(Exception exception, string typeString)
    {
        var message = $"{TimeString()}{GetLogPrefix("bold red", typeString)} {exception}";
        AnsiConsole.MarkupLine(message);
    }

    internal void Error(string exception, string typeString)
    {
        var message = $"{TimeString()}{GetLogPrefix("bold red", typeString)} {exception}";
        AnsiConsole.MarkupLine(message);
    }
}
