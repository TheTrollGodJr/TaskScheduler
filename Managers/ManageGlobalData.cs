namespace Managers;

/// <summary>
/// Global variables
/// </summary>
public static class GlobalData
{
    /// 
    /// DECLARING AND INITILIZING GLOBAL VARIABLES
    /// 
    

    public readonly static string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler"); // Path to the Program Files Dir
    public readonly static string jsonFilePath = Path.Combine(programDataPath, "tasks.json"); // Path to the tasks.json file
    public readonly static string lockPath = Path.Combine(programDataPath, ".lock"); // Path to the tasks.json lock file
    public static List<ScheduledTask>? TaskList; // List of all tasks retrieved from the tasks.json

    /// <summary>
    /// Constructor to setup files paths for logs and to get task info
    /// </summary>
    static GlobalData()
    {
        if (!Path.Exists(Path.Combine(programDataPath, "logs"))) Directory.CreateDirectory(Path.Combine(programDataPath, "logs")); // Create log subdir if it doesn't exist

        JsonHandler.UnlockJson(lockPath); // Remove any file lock on tasks.json
        TaskList = JsonHandler.GetJsonData(); // List of all tasks retrieved from the tasks.json
    }
}

/// <summary>
/// Class to hold task data
/// </summary>
public class ScheduledTask
{
    public string? TaskName;
    public string? Command;
    public string? Date;
    public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
    public bool Repeats;
    public int? TrueDate = null;
}