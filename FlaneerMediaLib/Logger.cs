using Spectre.Console;

namespace FlaneerMediaLib;

/// <summary>
/// 
/// </summary>
public class Logger
{
    private readonly LoggerFactory factory;
    private readonly string typeString;

    internal Logger(Type loggerType, LoggerFactory factory)
    {
        this.factory = factory;
        typeString = loggerType.ToString().Split('.').Last();
    }

    /// <summary>
    /// 
    /// </summary>
    public static Logger GetLogger(object obj)
    {
        return LoggerFactory.CreateLogger(obj);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Info(string s) => factory.Info(s, typeString);

    /// <summary>
    /// 
    /// </summary>
    public void Debug(string s) => factory.Debug(s, typeString);

    /// <summary>
    /// 
    /// </summary>
    public void Error(Exception exception) => factory.Error(exception, typeString);

    /// <summary>
    /// 
    /// </summary>
    public void Error(string error) => factory.Error(error, typeString);
}