using Managers.jsonHandler;

/// <summary>
/// Global list variable of all tasks
/// </summary>
public static class GlobalData {
    public static List<Task> TaskList = jsonHandler.GetJsonData(); // List of all tasks retrieved from the .json in AppData Roaming
}