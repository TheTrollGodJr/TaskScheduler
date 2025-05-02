//using Managers.jsonHandler;
using Managers;

namespace Managers;

/// <summary>
/// Global list variable of all tasks
/// </summary>
public static class GlobalData {
    public static string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler");
    //public static string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler"); // Path to the local AppData Roaming folder
    public static string jsonFilePath = Path.Combine(programDataPath, "tasks.json"); // Path to the tasks.json file holding task data in AppData Roaming
    public static string lockPath = Path.Combine(programDataPath, ".lock"); // Path to .lock file in AppData Roaming
    public static List<ScheduledTask> TaskList = JsonHandler.GetJsonData(); // List of all tasks retrieved from the tasks.json in AppData Roaming

    
}

public class ScheduledTask {
        public string TaskName;
        public string Command;
        public string Date;
        public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
        public bool Repeats;
    }