using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class CyclicalFrameCounterTests
{
    [Fact]
    public void TestGetCurrent()
    {
        var frameCounter = new CyclicalFrameCounter();
        var result = frameCounter.GetCurrent();
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void TestGetNext()
    {
        var frameCounter = new CyclicalFrameCounter();
        var result = frameCounter.GetNext();
        Assert.Equal(1, result);
    }
    
    [Fact]
    public void TestIncrement()
    {
        var frameCounter = new CyclicalFrameCounter();
        frameCounter.Increment();
        var result = frameCounter.GetCurrent();
        Assert.Equal(1, result);

        for (byte i = 0; i < byte.MaxValue; i++)
        {
            frameCounter.Increment();
        }
        //Check it cycles correctly
        Assert.Equal(1, frameCounter.GetCurrent());
    }
    
    [Fact]
    public void TestIncrementOperator()
    {
        var frameCounter = new CyclicalFrameCounter();
        frameCounter++;
        var result = frameCounter.GetCurrent();
        Assert.Equal(1, result);

        for (byte i = 0; i < byte.MaxValue; i++)
        {
            frameCounter++;
        }
        //Check it cycles correctly
        Assert.Equal(1, frameCounter.GetCurrent());
    }
    
    [Fact]
    public void TestSkipTo()
    {
        var frameCounter = new CyclicalFrameCounter();
        frameCounter.SkipTo(100);
        var result = frameCounter.GetCurrent();
        Assert.Equal(100, result);
        
        
        frameCounter.SkipTo(10);
        result = frameCounter.GetCurrent();
        Assert.Equal(10, result);
    }
    
    [Fact]
    public void TestMin()
    {
        var frameCounter = new CyclicalFrameCounter();
        
        Assert.Equal(0, frameCounter.Min(0,1));
        
        //This will cause the counter to cycle
        frameCounter.SkipTo(2);
        frameCounter.SkipTo(1);
        
        Assert.Equal(254, frameCounter.Min(0, 254));
    }
    
    [Fact]
    public void TestMax()
    {
        var frameCounter = new CyclicalFrameCounter();
        
        Assert.Equal(1, frameCounter.Max(0,1));
        
        //This will cause the counter to cycle
        frameCounter.SkipTo(2);
        frameCounter.SkipTo(1);
        
        Assert.Equal(0, frameCounter.Max(0, 254));
    }
    
    [Fact]
    public void TestIsOlder()
    {
        var frameCounter = new CyclicalFrameCounter();
        frameCounter.Increment();
        Assert.True(frameCounter.IsOlder(0));
        
        for (byte i = 0; i < byte.MaxValue; i++)
        {
            frameCounter++;
        }
        
        Assert.True(frameCounter.IsOlder(0));
    }
    
    [Fact]
    public void TestIsNewer()
    {
        var frameCounter = new CyclicalFrameCounter();
        frameCounter.Increment();
        Assert.True(frameCounter.IsNewer(2));
        
        for (byte i = 0; i < byte.MaxValue; i++)
        {
            frameCounter++;
        }
        
        Assert.True(frameCounter.IsNewer(2));
    }
}