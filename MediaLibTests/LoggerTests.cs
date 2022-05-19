using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class LoggerTests
{
    [Fact(Skip = "Only used to test formatting")]
    public void TestLogInfo()
    {
        var logger = Logger.GetLogger(this);
        logger.Info("Hello 123 ?%&");
    }

    [Fact(Skip = "Only used to test formatting")]
    public void TestLogDebug()
    {
        var logger = Logger.GetLogger(this);
        logger.Debug("Hello 123 ?%&");
    }

    [Fact(Skip = "Only used to test formatting")]
    public void TestLogError()
    {
        var logger = Logger.GetLogger(this);
        logger.Error("Hello 123 ?%&");
    }

}