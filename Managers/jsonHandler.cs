using Newtonsoft.Json;

namespace Managers.jsonHandler;

public static class jsonHandler {
    public static List<Task> GetJsonData() {
        string roamingAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShermanTaskScheduler");
        string jsonFilePath = Path.Combine(roamingAppDataPath, "tasks.json");
        if (!Directory.Exists(roamingAppDataPath)) Directory.CreateDirectory(roamingAppDataPath);
        if (!File.Exists(jsonFilePath)) {
            var emptyTaskData = new List<Task>();
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(emptyTaskData, Formatting.Indented));
            return emptyTaskData;
        }
        string jsonData = File.ReadAllText(jsonFilePath);
        if (string.IsNullOrEmpty(jsonData)) return new List<Task>();
        List<Task> tasks = JsonConvert.DeserializeObject<List<Task>>(jsonData);
        return tasks;
    }
}