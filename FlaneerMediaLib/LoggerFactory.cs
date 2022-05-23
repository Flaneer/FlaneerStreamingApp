using Spectre.Console;

namespace FlaneerMediaLib;

/// <summary>
/// Can be used to create and track loggers
/// </summary>
internal class LoggerFactory
{
    private static LoggerFactory instance = null!;

    // ReSharper disable once ConstantNullCoalescingCondition
    private static LoggerFactory Instance => instance ??= new LoggerFactory();

    private List<string> log = new List<string>();
    private List<string> perfStats = new List<string>();

    private Table logTable = new Table();
    private string displayMessage = "";
    
    private int logRows = 20;

    public LoggerFactory()
    {
        CreateLogTable();
    }

    public static Logger CreateLogger(object obj) => new Logger(obj.GetType(), Instance);

    private void CreateLogTable()
    {
        logTable = new Table();
        logTable.Centered();
        logTable.Expand = true;
        logTable.NoBorder();
        
        logTable.AddColumn(" ");
        logTable.AddRow(" ");
        logTable.AddRow(" ");
        Task.Run(() => AnsiConsole.Live(logTable).StartAsync(ctx =>
        {
            while (true)
            {
                lock (logTable)
                {
                    //TODO: add rules

                    for (int i = 1; i < logRows; i++)
                    {
                        var displayText = GenerateDisplayText(logRows);
                        logTable.Rows.Update(0,0,new Markup(displayText));
                    }
                    
                    var perfText = GeneratePerfText();
                    logTable.Rows.Update(1,0,new Markup(perfText));
                    
                    ctx.Refresh();
                }
            }
        }));
    }
    
    private string GeneratePerfText()
    {
        if (perfStats.Count == 0)
            return displayMessage;
        
        lock (perfStats)
        {
            string sep = "        ";
            displayMessage = string.Join(sep, perfStats);
            perfStats.Clear();
            return displayMessage;
        }
    }

    private string GenerateDisplayText(int logRows)
    {
        string displayText = "";
        var logsToDisplay = log.Take(new Range(Math.Max(log.Count - logRows, 0), log.Count)).ToList();
        for (int j = 0; j < Math.Min(logRows, logsToDisplay.Count); j++)
        {
            displayText += logsToDisplay[j];
        }

        return displayText;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Info(string s, string typeString)
    {
        log.Add($"[bold green]<INFO :{typeString}>[/] {s}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Debug(string s, string typeString)
    {
        log.Add($"[bold yellow]<DEBUG:{typeString}>[/] {s}\n");
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Error(Exception exception, string typeString)
    {
        log.Add($"[bold red]<ERROR:{typeString}>[/] {exception}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Error(string exception, string typeString)
    {
        log.Add($"[bold red]<ERROR:{typeString}>[/] {exception}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void LogPerfStat(string stat, object value)
    {
        perfStats.Add($"[bold blue]{stat.ToUpper()}[/] [white on blue]{value.ToString()}[/]");
    }
}