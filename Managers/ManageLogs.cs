using Serilog;

namespace Managers;

public static class LogManager
{
    /// 
    ///     GET FILE PATHS
    /// 
    private readonly static string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler", "logs");
    private readonly static string logPath = Path.Combine(logDir, "backend.log");
    private readonly static string frontLogPath = Path.Combine(logDir, "frontend.log");

    /// 
    ///     DECLARE LOG VARIABLES
    /// 
    public static Serilog.Core.Logger log;
    public static Serilog.Core.Logger frontLog;

    /// <summary>
    /// Initilize both log variables
    /// </summary>
    static LogManager()
    {

        log = new LoggerConfiguration().WriteTo.File(logPath, rollingInterval: RollingInterval.Day).CreateLogger();
        frontLog = new LoggerConfiguration().WriteTo.File(frontLogPath, rollingInterval: RollingInterval.Day).CreateLogger();
    }

    ///
    ///     LOG FUNCTIONS
    /// 

    /// <summary>
    /// Remove all previously created logs
    /// </summary>
    public static void ClearAllLogs()
    {

        string[] logFiles = Directory.GetFiles(logDir, "*.log", SearchOption.TopDirectoryOnly); // Get all *.log paths
        foreach (var file in logFiles) if (File.Exists(file)) File.Delete(file); // Remove log files
    }

    public static void CleanOldLogs()
    {

        string[] logFiles = Directory.GetFiles(logDir, "*.log", SearchOption.TopDirectoryOnly); // Get all *.log paths

        // Check all dates
        foreach (var file in logFiles)
        {

            string fileDate = Path.GetFileName(file); // Get log file name
            
            if (fileDate.Length >= 8) fileDate = fileDate.Substring(fileDate.Length - 12, 8); // Get the date from the filename
            else continue; // Cannot get datel ignore this file
            
            // Output parsed date from the file name
            if (DateTime.TryParseExact(fileDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {

                // Date is greater than 7 days ago
                if ((DateTime.Now - parsed).TotalDays > 7)
                {   // Remove old log file

                    File.Delete(file);
                    log.Information($"Removed Old Log File: {Path.GetFileName(file)}");
                }
            }
        }
    }
}