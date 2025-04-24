using System;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text.Json;
using System.Collections.Generic;

struct Task {
    public string TaskName;
    public string Command;
    public string Date;
    public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
    public bool Repeats;
}

class Program {
    private static DateTime parsedDate;
    static void Main(string[] args) {
        Task test = NewTask();
        Console.WriteLine($"Task Name: {test.TaskName}");
        Console.WriteLine($"Scheduled for: {test.Date}");
        Console.WriteLine($"Command: {test.Command}");
        Console.WriteLine($"Repeats: {test.Repeats}");
        if (test.Repeats) {
            Console.WriteLine($"Repeat Interval: {test.RepeatInterval}");
        }
    }

    static Task NewTask() {
        Task item = new Task();
        Console.WriteLine("Task Name: ");
        item.TaskName = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(item.TaskName)) {
            Console.WriteLine("Name cannot be empty. Try again: ");
            item.TaskName = Console.ReadLine();
        }

        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
        item.Date = Console.ReadLine();
        int DateCheckStatus = CheckDate(item.Date, "MM/dd HH:mm");
        while (DateCheckStatus != 0) {
            if (DateCheckStatus == 1) Console.WriteLine("Cannot be empty. Try again: ");
            if (DateCheckStatus == 2) Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            item.Date = Console.ReadLine();
            DateCheckStatus = CheckDate(item.Date, "MM/dd HH:mm");
        }   

        Console.WriteLine("Repeat Task? (Y/N): ");
        string inp = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(inp) || (inp.ToUpper() != "Y" && inp.ToUpper() != "N")) {
            Console.WriteLine("Invalid Input. Try Again: ");
            inp = Console.ReadLine();
        }
        if (inp == "Y") item.Repeats = true;
        else item.Repeats = false;

        if (item.Repeats) {
            Console.WriteLine("Repeat Interval (sec, min, hr, day, week, mon, year): ");
            inp = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(inp) || (inp.ToLower() != "sec" && inp.ToLower() != "min" && inp.ToLower() != "hr" && inp.ToLower() != "day" && inp.ToLower() != "week" && inp.ToLower() != "mon" && inp.ToLower() != "year")) {
                Console.WriteLine("Invalid Input. Try Again: ");
                inp = Console.ReadLine();
            }
            item.RepeatInterval = inp;
        }
        else item.RepeatInterval = null;
        Console.WriteLine("Terminal Command: ");
        item.Command = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(item.Command)) {
            Console.WriteLine("Cannot be empty. Try Again: ");
            item.Command = Console.ReadLine();
        }
        return item;
    }

    static int CheckDate(string date, string format) {
        if (string.IsNullOrWhiteSpace(date)) return 1; // null or blank
        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) return 2; // invlaid format
        return 0; // correct format
    }

    static List<Task> GetJsonData() {
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