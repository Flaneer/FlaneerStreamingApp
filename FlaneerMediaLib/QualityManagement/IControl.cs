namespace FlaneerMediaLib.QualityManagement;
/// <summary>
/// 
/// </summary>
public interface IControl
{
    
    /// <summary>
    /// 
    /// </summary>
    float Weight { get; }
    
    /// <summary>
    /// 
    /// </summary>
    void Update(float deviationRate);
}