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

    private Table logTable = new Table();

    public LoggerFactory()
    {
        CreateLogTable();
    }

    public static Logger CreateLogger(object obj) => new Logger(obj.GetType(), Instance);

    private void CreateLogTable()
    {
        var table = new Table();
        table.Centered();
        table.Expand = true;
        table.NoBorder();
        
        var col = table.AddColumn("");
        Thread.Sleep(1000);

        var perfStatsRow = table.AddRow("");
        Thread.Sleep(1000);

        var logRow = table.AddRow("");
        Thread.Sleep(1000);


        const int logRows = 10;
        Task.Run(() => AnsiConsole.Live(table).StartAsync(ctx =>
        {
            var i = 0;
            while (true)
            {
                perfStatsRow.Rows.Update(0, 0, new Text("row " + i));

                string displayText = "";
                var logsToDisplay = log.Take(new Range(Math.Max(log.Count - logRows, 0), log.Count)).ToList();
                for (int j = 0; j < Math.Min(logRows, logsToDisplay.Count); j++)
                {
                    displayText += logsToDisplay[j];
                }

                logRow.Rows.Update(1, 0, new Markup(displayText));
                ctx.Refresh();
            }
        }));
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Info(string s, string typeString)
    {
        log.Add($"[bold green](INFO :{typeString})[/] {s}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Debug(string s, string typeString)
    {
        log.Add($"[bold yellow](DEBUG:{typeString})[/] {s}\n");
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Error(Exception exception, string typeString)
    {
        log.Add($"[bold red](ERROR:{typeString})[/] {exception}\n");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Error(string exception, string typeString)
    {
        log.Add($"[bold red](ERROR:{typeString})[/] {exception}\n");
    }
}