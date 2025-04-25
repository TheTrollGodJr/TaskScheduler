using Newtonsoft.Json;

namespace Managers.jsonHandler;

/// <summary>
/// Handles reading/writing to the tasks.json in AppData Roaming
/// </summary>

public static class jsonHandler {
    
    /// <summary>
    /// Read and return the tasks.json data from AppData Roaming that holds task information
    /// </summary>
    /// <returns>A list of Task structs</returns>
    public static List<Task> GetJsonData() {

        // Get AppData Roaming path and .json file path
        string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler");
        string jsonFilePath = Path.Combine(roamingAppDataPath, "tasks.json");

        if (!Directory.Exists(roamingAppDataPath)) Directory.CreateDirectory(roamingAppDataPath); // Create a directory in AppData Roaming if it doesn't exist
        
        // Check if the tasks.json exists, if not, create it
        if (!File.Exists(jsonFilePath)) {

            var emptyTaskData = new List<Task>(); // Create an empty task list object
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(emptyTaskData, Formatting.Indented)); // Create a new empty tasks.json
            return emptyTaskData; // Return the new list object
        }

        string jsonData = File.ReadAllText(jsonFilePath); // Read data from the json
        if (string.IsNullOrEmpty(jsonData)) return new List<Task>(); // If the json is empty, create and return an empty Task list
        
        List<Task> tasks = JsonConvert.DeserializeObject<List<Task>>(jsonData); // Convert the json string to a Task list
        return tasks; // Return the Task list
    }

    /// <summary>
    /// Saves the global TaskList data to a .json in AppData Roaming
    /// </summary>
    public static void SaveJsonData() {
        
        // Get AppData Roaming path and .json file path
        string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler");
        string jsonFilePath = Path.Combine(roamingAppDataPath, "tasks.json");
        
        string data = JsonConvert.SerializeObject(GlobalData.TaskList, Formatting.Indented); // Convert the Task List to a string
        
        File.WriteAllText(jsonFilePath, data); // Save the task data
    }
}