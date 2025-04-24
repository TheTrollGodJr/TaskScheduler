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
using Managers.jsonHandler;
using Managers.TaskManager;

public struct Task {
    public string TaskName;
    public string Command;
    public string Date;
    public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
    public bool Repeats;
}

class Program {
    
    static void Main(string[] args) {
        Task test = TaskManager.NewTask();
        Console.WriteLine($"Task Name: {test.TaskName}");
        Console.WriteLine($"Scheduled for: {test.Date}");
        Console.WriteLine($"Command: {test.Command}");
        Console.WriteLine($"Repeats: {test.Repeats}");
        if (test.Repeats) {
            Console.WriteLine($"Repeat Interval: {test.RepeatInterval}");
        }
    }

    
    
}