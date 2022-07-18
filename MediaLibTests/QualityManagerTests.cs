using System;
using System.Threading;
using FlaneerMediaLib;
using FlaneerMediaLib.QualityManagement;
using Xunit;

namespace MediaLibTests;

[Collection("Sequential")]
public class QualityManagerTests
{
    
    [Fact]
    public void TestQualityManagerInit()
    {
        var measure = new TestQualityMeasure();
        var control = new TestControl();

        var qualityManager = new QualityManager();
        Assert.Empty(qualityManager.measures);
        Assert.Empty(qualityManager.controls);
        
        qualityManager.AddControl(control);
        Assert.Equal(qualityManager.controls[0], control);
        
        qualityManager.AddMeasure(measure);
        Assert.Equal(qualityManager.measures[0], measure);

    }
    
    [Fact]
    public void TestQualityManagerSingleControlSingleMeasureEasy()
    {
        var measure = new TestQualityMeasure();
        var control = new TestControl();

        var qualityManager = new QualityManager();
        qualityManager.AddControl(control);
        qualityManager.AddMeasure(measure);

        Assert.Equal(0, measure.Score);
        Assert.Equal(0, control.Weight);
        Assert.Equal(30, control.FakeFPS);
        
        qualityManager.UpdateMeasuresAndControls();
        
        Assert.Equal(1, measure.Score);
        Assert.Equal(0, control.Weight);
        Assert.Equal(30, control.FakeFPS);
    }
    
    [Fact]
    public void TestQualityManagerSingleControlSingleMeasureHard()
    {
        var measure = new TestQualityMeasure();
        var control = new TestControl();

        measure.SwitchToHard();

        var qualityManager = new QualityManager();
        qualityManager.AddControl(control);
        qualityManager.AddMeasure(measure);

        Assert.Equal(0, measure.Score);
        Assert.Equal(0, control.Weight);
        Assert.Equal(30, control.FakeFPS);
        
        qualityManager.UpdateMeasuresAndControls();
        
        Assert.Equal(1, measure.Score);
        Assert.Equal(-0.8, control.Weight);
        Assert.Equal(29.2, control.FakeFPS);
    }
}
