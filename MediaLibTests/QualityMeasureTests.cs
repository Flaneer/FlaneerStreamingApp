using FlaneerMediaLib.QualityManagement;

namespace MediaLibTests;

public class TestQualityMeasure: IQualityMeasure
{
    private float score = 0;
    public float Score => score;

    private float target = 5;
    public float Target => target;
    
    private float acceptableDeviationRate = 10;
    public float AcceptableDeviationRate => acceptableDeviationRate;
    
    public void Update()
    {
        score += 1;
    }
    
    public void SwitchToHard()
    {
        acceptableDeviationRate = 0;
    }
}