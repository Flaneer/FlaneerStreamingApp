using System.Net.Sockets;

namespace FlaneerMediaLib;

/// <summary>
/// 
/// </summary>
public class Logger
{
    /// <summary>
    /// 
    /// </summary>
    public static Logger GetCurrentClassLogger()
    {
        return new Logger();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Info(string s)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socketException"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Error(Exception exception)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Debug(string s)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socketException"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Error(string socketException)
    {
        throw new NotImplementedException();
    }
}