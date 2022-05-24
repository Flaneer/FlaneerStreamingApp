namespace FlaneerMediaLib.Logging;

/// <summary>
/// The logger class for the streaming app
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
    /// Gets a class based logger
    /// </summary>
    public static Logger GetLogger(object obj)
    {
        return LoggerFactory.CreateLogger(obj);
    }

    /// <summary>
    /// Logs at the info level
    /// </summary>
    public void Info(string s) => factory.Info(s, typeString);

    /// <summary>
    /// Logs at the debug level
    /// </summary>
    public void Debug(string s) => factory.Debug(s, typeString);

    /// <summary>
    /// Logs at the error level
    /// </summary>
    public void Error(Exception exception) => factory.Error(exception, typeString);

    /// <summary>
    /// Logs at the error level
    /// </summary>
    public void Error(string error) => factory.Error(error, typeString);
}