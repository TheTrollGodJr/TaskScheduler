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
        //List<Task> tasks = jsonHandler.GetJsonData();
        var options = new List<string> { "Create Task", "View Tasks", "Edit Task", "Remove Task", "Exit" };
        
        while (true) {
            int choice = ShowMenu(options, "Welcome to Task Scheduler");

            //Console.Clear();
            switch (choice) {
                case 0: // Add Task
                    TaskManager.NewTask();
                    jsonHandler.SaveJsonData();
                    break;
                case 1: // View Tasks
                    ViewTasks();
                    break; 
                case 2: // Edit Tasks
                    break;
                case 3: // Remove Task
                    RemoveMenu();
                    break;
                case 4: // Exit
                    Environment.Exit(0); break;
                default: break;
            }
        }
    }

    static void ViewTasks() {
        Console.Clear();
        Console.WriteLine("All Tasks\n(Press any key to go back)\n");

        if (GlobalData.TaskList.Count == 0) {
            Console.WriteLine("  No Tasks");
            Console.ReadKey();
            return;
        }

        Console.BackgroundColor = ConsoleColor.Gray;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" Task Name |    date     | Repeat? | Intval | Command ");
        Console.ResetColor();
        foreach (var item in GlobalData.TaskList) {
            Console.WriteLine($"  {FixedLength(item.TaskName, 8)} | {FixedLength(item.Date, 11)} | {FixedLength(item.Repeats.ToString(), 7)} | {FixedLength(item.RepeatInterval ?? "Null", 6)} | {item.Command}");
        }
        Console.ReadKey();
    }

    static string FixedLength(string str, int len) {
        if (str.Length > len) return str.Substring(0, len);
        else if (str.Length < len) return str.PadRight(len);
        return str;
    }

    static int ShowMenu(List<string> options, string prompt = "Choose an option:") {
        int selected = 0;
        ConsoleKey key;

        do {
            Console.Clear();
            Console.WriteLine(prompt + "\n");

            for (int i = 0; i < options.Count; i++) {
                if (i == selected) Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"  {options[i]}");

                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + options.Count) % options.Count;
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % options.Count;

        } while (key != ConsoleKey.Enter);

        return selected;
    }

    /// <summary>
    /// Displays a menu to select a task to remove
    /// </summary>
    static void RemoveMenu() {
        // Initialize variables
        List<string> taskNames = TaskManager.GetTaskNames(); // Get task names
        ConsoleKey key; // Console key object
        int selected = 0; // Selected task index

        // If there are no saved tasks
        if (taskNames.Count == 0) {

            // Display message then wait for any key to return to the main menu
            Console.Clear();
            Console.WriteLine("Select a Task to remove\n(Press any key to go back)\n\n  No Tasks");
            Console.ReadKey();
            return;
        }

        // Show selection menu to select a task to remove
        do {

            Console.Clear();
            Console.WriteLine("Select a Task to remove\n(Press ESC to go back)\n");

            // Show all task names
            for (int i = 0; i < taskNames.Count; i++) {

                if (i == selected) Console.ForegroundColor = ConsoleColor.Green; // Highlight selected task in green

                Console.WriteLine($"  {taskNames[i]}");
                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key; // Get current key

            // Movement options using keyboard
            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + taskNames.Count) % taskNames.Count; // Select up
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % taskNames.Count; // Select down
            else if (key == ConsoleKey.Enter) { // Confirm deletion selection

                if (RemoveConfirmation(taskNames[selected])) { // Run function to confirm deletion

                    // Remove task
                    TaskManager.RemoveItem(taskNames[selected]);
                    taskNames.RemoveAt(selected);
                    }

                selected = 0; // Reset selection index
            }

        } while (key != ConsoleKey.Escape); // Look while ESC is not pressed
    }
    
    /// <summary>
    /// A confirmation screen to confirm the deletion of a task
    /// </summary>
    /// <param name="name">The name of the task to be deleted</param>
    /// <returns>True if it should be deleted, false if it shouldn't</returns>
    static bool RemoveConfirmation(string name) {

        // Reset and write to the console
        Console.Clear();
        Console.WriteLine($"Are you sure you want to delete task: {name}? (YES/NO)");

        string inp = Console.ReadLine().ToUpper(); // Get user input

        // Make sure the input is valid; either YES or NO
        while (inp != "YES" && inp != "NO") {

            Console.WriteLine("Invalid Input. Try Again (YES/NO)");
            inp = Console.ReadLine().ToUpper();
        }

        // Return true if YES, return false if NO
        if (inp == "YES") return true;
        return false;
    }
}