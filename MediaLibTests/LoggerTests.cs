using System;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace MediaLibTests;

public class LoggerTests
{
    [Fact]
    public void TestLogger()
    {
        string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }

        var x = NameOfCallingClass();
    }
}