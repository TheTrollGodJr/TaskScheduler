//using Managers.jsonHandler;
using Managers;
using Serilog;

namespace Managers;

/// <summary>
/// Global list variable of all tasks
/// </summary>
public static class GlobalData
{
    public readonly static string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler");
    //public static string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler"); // Path to the local AppData Roaming folder
    public readonly static string jsonFilePath = Path.Combine(programDataPath, "tasks.json"); // Path to the tasks.json file holding task data in AppData Roaming
    public readonly static string lockPath = Path.Combine(programDataPath, ".lock"); // Path to .lock file in AppData Roaming
    public static List<ScheduledTask>? TaskList = JsonHandler.GetJsonData(); // List of all tasks retrieved from the tasks.json in AppData Roaming
    public readonly static string logPath = Path.Combine(programDataPath, "backend.log");
    public readonly static string frontLogPath = Path.Combine(programDataPath, "frontend.log");
    public static Serilog.Core.Logger log = new LoggerConfiguration().WriteTo.File(logPath, rollingInterval: RollingInterval.Month).CreateLogger();
    public static Serilog.Core.Logger frontLog = new LoggerConfiguration().WriteTo.File(frontLogPath, rollingInterval: RollingInterval.Month).CreateLogger();
}

public class ScheduledTask {
        public string TaskName;
        public string Command;
        public string Date;
        public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
        public bool Repeats;
        public int? TrueDate = null;
    }