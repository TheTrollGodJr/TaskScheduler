using Newtonsoft.Json;
using Managers;

//namespace Managers.jsonHandler;
namespace Managers;

/// <summary>
/// Handles reading/writing to the tasks.json in AppData Roaming
/// </summary>

public static class JsonHandler {
    
    /// <summary>
    /// Read and return the tasks.json data from AppData Roaming that holds task information
    /// </summary>
    /// <returns>A list of Task structs</returns>
    public static List<ScheduledTask>? GetJsonData() {

        if (!Directory.Exists(GlobalData.programDataPath)) Directory.CreateDirectory(GlobalData.programDataPath); // Create a directory in AppData Roaming if it doesn't exist
        
        // Check if the tasks.json exists, if not, create it
        if (!File.Exists(GlobalData.jsonFilePath)) {

            var emptyTaskData = new List<ScheduledTask>(); // Create an empty task list object
            SaveJsonData(true);

            return emptyTaskData; // Return the new list object
        }

        // read file; lock while reading
        if (!WaitForLock(GlobalData.lockPath))
        {
            GlobalData.log.Error("Json File Timeout; Waited Too Long For it to Unlock");
            return null;
        }

        if (!LockJson(GlobalData.lockPath))
        {
            GlobalData.log.Error("Could Not Lock Json File");
            return null;
        }
        string jsonData = File.ReadAllText(GlobalData.jsonFilePath); // Read data from the json
        GlobalData.frontLog.Information("Got Json data");

        if (!UnlockJson(GlobalData.lockPath))
        {
            GlobalData.frontLog.Error("Could not Unlock Json File");
            return null;
        }

        if (string.IsNullOrEmpty(jsonData)) return new List<ScheduledTask>(); // If the json is empty, create and return an empty Task list
        
        List<ScheduledTask>? taskList = JsonConvert.DeserializeObject<List<ScheduledTask>>(jsonData); // Convert the json string to a Task list
        return taskList; // Return the Task list
    }

    /// <summary>
    /// Saves the global TaskList data to a .json in AppData Roaming
    /// </summary>
    public static bool SaveJsonData(bool saveEmptyJson = false)
    {
        string data;
        if (saveEmptyJson == false) data = JsonConvert.SerializeObject(GlobalData.TaskList, Formatting.Indented); // Convert the Task List to a string
        else
        {
            var rawData = new List<ScheduledTask>();
            data = JsonConvert.SerializeObject(rawData, Formatting.Indented); // Convert the Task List to a string
        }
            

        // Lock json file and save changes
        if (!WaitForLock(GlobalData.lockPath))
        {
            GlobalData.log.Error("Json File Timeout; Waited Too Long For it to Unlock");
            return false;
        }

        if (!LockJson(GlobalData.lockPath))
        {
            GlobalData.log.Error("Could Not Lock Json File");
            return false;
        }

        File.WriteAllText(GlobalData.jsonFilePath, data); // Save the task data
        GlobalData.log.Information("Saving Json Data");

        if (!UnlockJson(GlobalData.lockPath))
        {
            GlobalData.log.Error("Could Not Unlock Json File");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Create a lock on a file file so no other processes can access it
    /// </summary>
    /// <param name="lockPath">File path where the lockfile should go</param>
    /// <returns>A bool determining whether it was successful or not</returns>
    public static bool LockJson(string lockPath) {

        // Try to create a lock file
        try {

            if (File.Exists(lockPath)) return false; // Lock file already exists, cannot create a lock

            File.Create(lockPath).Dispose(); // Create lock file
            return true; // Success
        }
        // Return false if there are any problem creating the file
        catch {return false;}
    }

    /// <summary>
    /// Deletes a lock file allowing other processes to access a file
    /// </summary>
    /// <param name="lockPath"File path for the lock file to be deleted></param>
    /// <returns>A bool determining whether it was successful or not</returns>
    public static bool UnlockJson(string lockPath) {

        // Try to delete a lock file
        try {

            // If the lock file exists, remove it and return success
            if (File.Exists(lockPath)) {

                File.Delete(lockPath);
                return true;
            }

            return false; // Could not remove lock; lock file doesn't exist
        }
        // Return false if any errors occur
        catch {return false;}
    }

    /// <summary>
    /// Waits for a lockfile to be removed
    /// </summary>
    /// <param name="lockPath">File path to the lock file</param>
    /// <param name="timeout">Timeout in ms; set to 1000ms by default</param>
    /// <param name="waitInterval">Time in ms between checking the lock file</param>
    /// <returns>True if it waited successfully, false if it timed out</returns>
    public static bool WaitForLock(string lockPath, int timeout = 1000, int waitInterval = 50) {

        int waited = 0; // Total time waited

        // Wait for lock to be remove or timeout
        while (File.Exists(lockPath)) {

            if (waited > timeout) return false; // Return false if the wait timed out

            Thread.Sleep(waitInterval); // Wait a specified amount of time (ms)
            waited += waitInterval; // Add to total wait time
        }

        return true; // Waited successfully
    }

}