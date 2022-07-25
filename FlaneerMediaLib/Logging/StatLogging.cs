namespace FlaneerMediaLib.Logging;

/// <summary>
/// Logs stats specifically, will generate a single string that will be updated 
/// </summary>
public class StatLogging
{
    private static StatLogging instance = null!;

    // ReSharper disable once ConstantNullCoalescingCondition
    private static StatLogging Instance => instance ??= new StatLogging();
    
    private readonly Dictionary<string, object> perfStats = new();
    private string displayMessage = "";

    /// <summary>
    /// Logs an always visible stat, intended for continuously updating values.
    /// <remarks>This will use the <code>ToString()</code> method for <see cref="value"/></remarks>
    /// </summary>
    public static void LogPerfStat(string stat, object value)
    {
        Instance.perfStats.TryAdd(stat, value);
        Instance.perfStats[stat] = value;
    }
   
    /// <summary>
    /// Returns a string with the latest values of all the stats split bit a series of spaces
    /// </summary>
    public static string GetPerfStats()
    {
        if (Instance.perfStats.Count == 0)
            return Instance.displayMessage;

        Instance.displayMessage = "";
        
        string sep = "        ";
        //TODO fix this race condition
        foreach (var kvp in Instance.perfStats)
        {
            Instance.displayMessage += kvp.Key + ":" + kvp.Value + sep;
        }
        return Instance.displayMessage;
    }
}
