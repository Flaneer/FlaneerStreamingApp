namespace FlaneerMediaLib.QualityManagement;
/// <summary>
/// Interface for all Control classes to implement
/// </summary>
public interface IControl
{
    
    /// <summary>
    /// The current weight of the control.
    /// <remarks>The higher the weight, the highest probability the control will be modified.</remarks>
    /// </summary>
    float Weight { get; }
    
    /// <summary>
    /// Function to update the control (aka some parameters of the streaming server)
    /// <remarks>The higher (in absolute value) the deviation rate, the bigger the modification.</remarks>
    /// </summary>
    void Update(float deviationRate);
}