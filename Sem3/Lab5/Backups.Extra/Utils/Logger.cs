namespace Backups.Extra.Utils;

public abstract class Logger
{
    private const string LogLevelChar = "IWE";
    public bool WriteDate { get; set; } = true;
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    public void Info(string? message = null, object? obj = null) => Log(LogLevel.Info, message, obj);
    public void Warning(string? message = null, object? obj = null) => Log(LogLevel.Warning, message, obj);
    public void Error(string? message = null, object? obj = null) => Log(LogLevel.Error, message, obj);

    protected abstract void Write(string message);

    private void Log(LogLevel type, string? str, object? obj)
    {
        if (type.GetHashCode() < LogLevel.GetHashCode())
            return;
        string result = WriteDate ? $"[{DateTime.Now}] " : string.Empty;
        result += $"[{LogLevelChar[type.GetHashCode()]}]";
        if (!string.IsNullOrWhiteSpace(str))
            result += $" {str}";
        if (obj is ILoggable loggableObj)
            result += $" {loggableObj.LogString()}";
        else if (obj is not null)
            result += $" {obj.ToString()}";
        Write(result);
    }
}