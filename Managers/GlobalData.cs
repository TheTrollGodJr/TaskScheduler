using Managers.jsonHandler;

/// <summary>
/// Global list variable of all tasks
/// </summary>
public static class GlobalData {
    public static string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler"); // Path to the local AppData Roaming folder
    public static string jsonFilePath = Path.Combine(roamingAppDataPath, "tasks.json"); // Path to the tasks.json file holding task data in AppData Roaming
    public static string lockPath = Path.Combine(roamingAppDataPath, ".lock"); // Path to .lock file in AppData Roaming
    public static List<Task> TaskList = jsonHandler.GetJsonData(); // List of all tasks retrieved from the tasks.json in AppData Roaming
}