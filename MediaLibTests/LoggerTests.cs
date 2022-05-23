using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaneerMediaLib;
using Spectre.Console;
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

    class Foo{}
    class Bar{}
    class MyClass{}
    class FakeClass {}

    private static readonly Random Random = new Random();

    public static string Shuffle(IList<string> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }

        var ret = "";
        for (int i = 0; i < Random.Next(list.Count); i++)
        {
            ret += list[i] + " ";
        }

        return ret;
    }
    
    [Fact]
    public void TestFullLogging()
    {
        var loggers = new[]
        {
            Logger.GetLogger(new Foo()), Logger.GetLogger(new Bar()), Logger.GetLogger(new MyClass()),
            Logger.GetLogger(new FakeClass())
        };
        //Set logging chaos thread
        Task.Run(() =>
        {
            var words = "The quick brown fox jumps over the lazy dog".Split(' ').ToList();
            while (true)
            {
                var logger = loggers[Random.Next(3)];

                switch (Random.Next(3))
                {
                    case 0:
                        logger.Info(Shuffle(words));
                        break;
                    case 1:
                        logger.Debug(Shuffle(words));
                        break;
                    case 2:
                        logger.Error(Shuffle(words));
                        break;
                    default:
                        logger.Info(Shuffle(words));
                        break;
                }

                var randomWaitTime = Random.Next(10);
                
                logger.LogPerfStat("FPS", 60 + Random.Next(10));
                logger.LogPerfStat("ABC", 100 + Random.Next(99));
                logger.LogPerfStat("XYZ", $"{Random.Next(99)}%");
                
                Thread.Sleep(200);
            }
        });
        Thread.Sleep(20000);
    }
}