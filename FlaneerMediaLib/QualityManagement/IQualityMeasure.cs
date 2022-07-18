using System.Data;

namespace FlaneerMediaLib.QualityManagement;
/// <summary>
/// 
/// </summary>
public interface IQualityMeasure
{
    /// <summary>
    /// 
    /// </summary>
    float Score { get; }
    
    /// <summary>
    ///
    /// </summary>
    float Target { get; }
    
    /// <summary>
    /// Acceptable deviation rate from the target
    /// <remarks>Computed as abs(score - target)/(target)</remarks>
    /// </summary>
    float AcceptableDeviationRate { get; }
    
    /// <summary>
    /// 
    /// </summary>
    void Update();
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    float GetCurrentDeviationRate()
    {
        return (Score - Target) / Target;
    }
}