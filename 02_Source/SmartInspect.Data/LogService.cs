namespace SmartInspect.Data;

public sealed class LogService
{
    private readonly object sync = new();
    public string DirectoryPath { get; }
    public string? LastWriteError { get; private set; }
    public LogService(string? directory = null) => DirectoryPath = directory ?? Path.Combine(AppContext.BaseDirectory, "Logs");
    public void Info(string message) => Write("INFO", message);
    public void Warning(string message) => Write("WARNING", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        lock (sync)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                var now = DateTime.Now;
                File.AppendAllText(Path.Combine(DirectoryPath, $"SmartInspect_{now:yyyy-MM-dd}.log"), $"{now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message.Replace('\r', ' ').Replace('\n', ' ')}{Environment.NewLine}");
                LastWriteError = null;
            }
            catch (Exception ex) { LastWriteError = ex.Message; }
        }
    }
}
