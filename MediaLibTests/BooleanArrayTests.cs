using System;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class BooleanArrayTests
{
    [Fact]
    public void TestToByte()
    {
        var testFull = new []{false, true, true, false, false, true, true, false}; //0x01100110 == 102
        var testFullResult = BooleanArrayUtils.ConvertBoolArrayToByte(testFull);
        Assert.Equal(102, testFullResult);
        
        var testPartial = new []{true}; // 0x10000000 == 128
        var testPartialResult = BooleanArrayUtils.ConvertBoolArrayToByte(testPartial);
        Assert.Equal(128, testPartialResult);
        
        var testOverflowing = new []{false, true, true, false, false, true, true, false, true};
        Action testOverflowingFunc = () => BooleanArrayUtils.ConvertBoolArrayToByte(testOverflowing);
        Assert.Throws<Exception>(testOverflowingFunc);
        
        var testEmpty = new bool []{ };
        var testEmptyResult = BooleanArrayUtils.ConvertBoolArrayToByte(testEmpty);
        Assert.Equal(0, testEmptyResult);

        var testParamResult = BooleanArrayUtils.ConvertBoolArrayToByte(true, false, true); // 0x10100000 == 160
        Assert.Equal(160, testParamResult);
    }

    [Fact]
    public void TestToBoolArray()
    {
        var expectedFrom102 = new []{false, true, true, false, false, true, true, false};
        Assert.Equal(expectedFrom102, BooleanArrayUtils.ConvertByteToBoolArray(102));
    }
}
