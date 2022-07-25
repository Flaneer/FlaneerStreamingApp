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
    
    private string TimeStringNoMarkup() => $"({DateTime.Now.ToString("HH:mm:ss")})";
    private string TimeString() => $"[gray]{TimeStringNoMarkup()}[/]";

    private string GetMarkupLogPrefix(string formatting, string typeString)
    {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(3);
        var method = $"{sf.GetMethod().Name}";
        var fileLineNumber = $"{sf.GetFileLineNumber()}";

        return $"{TimeString()}[{formatting}]<{typeString}:{fileLineNumber}[/][gray]({method})[/][{formatting}]>[/]";
    }

    private string GetNonMarkupLogPrefix(string typeString)
    {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(3);
        var method = $"{sf.GetMethod().Name}";
        var fileLineNumber = $"{sf.GetFileLineNumber()}";
        return $"{TimeStringNoMarkup()}<{typeString}:{fileLineNumber}({method})>";
    }
    
    internal static Logger CreateLogger(object obj) => new Logger(obj.GetType(), Instance);

    internal void Info(string s, string typeString)
    {
        //var message = $"{GetMarkupLogPrefix("bold green", typeString)} {s}";
        var message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
        try
        {
            AnsiConsole.WriteLine(message);
            //AnsiConsole.MarkupLine(message);
        }
        catch (InvalidOperationException)
        {
            message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
            AnsiConsole.WriteLine(message);
        }
    }
    
    internal void Debug(string s, string typeString)
    {
        //var message = $"{GetMarkupLogPrefix("bold green", typeString)} {s}";
        var message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
        try
        {
            AnsiConsole.WriteLine(message);
            //AnsiConsole.MarkupLine(message);
        }
        catch (InvalidOperationException)
        {
            message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
            AnsiConsole.WriteLine(message);
        }
    }
    
    internal void Error(Exception exception, string typeString)
    {
        var message = $"{GetMarkupLogPrefix("bold red", typeString)} {exception}";
        try
        {
            AnsiConsole.WriteLine(message);
            //AnsiConsole.MarkupLine(message);
        }
        catch (InvalidOperationException)
        {
            message = $"{GetNonMarkupLogPrefix(typeString)} {exception}";
            AnsiConsole.WriteLine(message);
        }
    }

    internal void Error(string exception, string typeString)
    {
        var message = $"{GetMarkupLogPrefix("bold red", typeString)}";
        AnsiConsole.Markup(message);
        AnsiConsole.Write(exception + "\n");
    }

    internal void Trace(string s, string typeString)
    {
        //var message = $"{GetMarkupLogPrefix("bold green", typeString)} {s}";
        var message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
        try
        {
            AnsiConsole.WriteLine(message);
            //AnsiConsole.MarkupLine(message);
        }
        catch (InvalidOperationException)
        {
            message = $"{GetNonMarkupLogPrefix(typeString)} {s}";
            AnsiConsole.WriteLine(message);
        }
    }

    internal void Time(string processName, TimeSpan timeTaken)
    {
        var message = $"{GetMarkupLogPrefix("bold blue", processName)} {timeTaken:ss:ffff}";
        AnsiConsole.MarkupLine(message);
    }
}
