namespace FlaneerMediaLib.QualityManagement;

/// <summary>
/// 
/// </summary>
public class QualityManager: IService
{

    internal readonly List<IQualityMeasure> measures = new List<IQualityMeasure>() ;
    internal readonly List<IControl> controls = new List<IControl>();
    private bool receiving;

    /// <summary>
    /// 
    /// </summary>
    public QualityManager()
    {
        Task.Run(Management);
    }
    
    private void Management()
    {
        while (receiving)
        {
            Thread.Sleep(1);
            controls.Sort((x, y) => x.Weight.CompareTo(y.Weight));
            UpdateMeasuresAndControls();
        }
    }

    internal void UpdateMeasuresAndControls()
    {
        foreach (var measure in measures)
        {
            measure.Update();
            var measureDeviationRate = measure.GetCurrentDeviationRate();
                
            foreach (var control in controls)
            {
                if (Math.Abs(measureDeviationRate) > measure.AcceptableDeviationRate)
                {
                    control.Update(measureDeviationRate);
                    break;
                }
            }
        }  
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddMeasure(IQualityMeasure qualityMeasure) => measures.Add(qualityMeasure);

    /// <summary>
    /// 
    /// </summary>
    public void AddControl(IControl control) => controls.Add(control);

    /// <summary>
    /// 
    /// </summary>
    public void Start() => receiving = true;
    
    /// <summary>
    /// 
    /// </summary>
    public void Stop() => receiving = false;
}