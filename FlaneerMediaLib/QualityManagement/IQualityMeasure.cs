using System.Data;

namespace FlaneerMediaLib.QualityManagement;
/// <summary>
/// Interface for all Measures classes to implement
/// </summary>
public interface IQualityMeasure
{
    /// <summary>
    /// Current value of the measure
    /// </summary>
    float Score { get; }
    
    /// <summary>
    /// Target of the measure
    /// </summary>
    float Target { get; }
    
    /// <summary>
    /// Acceptable deviation rate from the target
    /// <remarks>Computed as abs(score - target)/(target)</remarks>
    /// </summary>
    float AcceptableDeviationRate { get; }
    
    /// <summary>
    /// Function to update the measure (will gather this on the client side)
    /// </summary>
    void Update();
    
    /// <summary>
    /// Function to compute the current deviation rate
    /// </summary>
    /// <returns></returns>
    float GetCurrentDeviationRate()
    {
        return (Score - Target) / Target;
    }
}