using Serilog;

namespace Managers;

public static class LogManager
{
    private readonly static string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler", "logs");
    private readonly static string logPath = Path.Combine(logDir, "backend.log");
    private readonly static string frontLogPath = Path.Combine(logDir, "frontend.log");
    public static Serilog.Core.Logger log;
    public static Serilog.Core.Logger frontLog;
    static LogManager()
    {
        log = new LoggerConfiguration().WriteTo.File(logPath, rollingInterval: RollingInterval.Day).CreateLogger();
        frontLog = new LoggerConfiguration().WriteTo.File(frontLogPath, rollingInterval: RollingInterval.Day).CreateLogger();
    }

    public static void ClearAllLogs()
    {
        string[] logFiles = Directory.GetFiles(logDir, "*.log", SearchOption.TopDirectoryOnly);
        foreach (var file in logFiles) if (File.Exists(file)) File.Delete(file);
    }

    public static void CleanOldLogs()
    {
        string[] logFiles = Directory.GetFiles(logDir, "*.log", SearchOption.TopDirectoryOnly);

        foreach (var file in logFiles)
        {
            string fileDate = Path.GetFileName(file);
            if (fileDate.Length >= 8) fileDate = fileDate.Substring(fileDate.Length - 8);
            else continue;

            if (DateTime.TryParseExact(fileDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                if ((DateTime.Now - parsed).TotalDays > 7)
                {
                    File.Delete(file);
                    log.Information($"Removed Old Log File: {Path.GetFileName(file)}");
                }
            }
        }
    }
}