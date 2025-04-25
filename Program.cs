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
using System.Runtime.InteropServices;

public struct Task {
    public string TaskName;
    public string Command;
    public string Date;
    public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
    public bool Repeats;
}

class Program {
    
    static void Main(string[] args) {
        List<Task> tasks = jsonHandler.GetJsonData();
        var options = new List<string> { "Create Task", "View Tasks", "Edit Task", "Remove Task", "Exit" };
        int choice = ShowMenu(options, "Welcome to Task Scheduler");

        Console.Clear();
        switch (choice) {
            case 0: // Add Task
                tasks.Add(TaskManager.NewTask());
                jsonHandler.SaveJsonData(tasks);
                break;
            case 1: // View Tasks 
                break; 
            case 2: // Edit Tasks
                break;
            case 3: // Remove Task
                break;
            case 4: // Exit
                break;
            default: break;
        }
    }

    public static int ShowMenu(List<string> options, string prompt = "Choose an option:") {
        int selected = 0;
        ConsoleKey key;

        do {
            Console.Clear();
            Console.WriteLine(prompt + "\n");

            for (int i = 0; i < options.Count; i++) {
                if (i == selected) {
                    //Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine($"  {options[i]}");

                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + options.Count) % options.Count;
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % options.Count;

        } while (key != ConsoleKey.Enter);

        return selected;
    }

    static void Menu() {
        
    }
    
}