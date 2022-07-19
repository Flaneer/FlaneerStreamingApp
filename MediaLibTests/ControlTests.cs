using System.Net.Mail;
using FlaneerMediaLib.QualityManagement;

namespace MediaLibTests;

public class TestControl: IControl
{
    private float weight = 0;
    public float Weight => weight;

    private float fakeFPS = 30;
    public float FakeFPS => fakeFPS;
    
    public void Update(float deviationRate)
    {
        weight += deviationRate;
        //
        // Some actions on the control, dependant of deviationRate
        //
        fakeFPS += deviationRate;
    }
}