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
using System.Runtime.Serialization;

public class Task {
    public string TaskName;
    public string Command;
    public string Date;
    public string? RepeatInterval; //second, minute, hour, day, week, month, year, null (if does not repeat)
    public bool Repeats;
}

class Program {
    
    static void Main(string[] args) {
        jsonHandler.UnlockJson(GlobalData.lockPath);

        var options = new List<string> { "Create Task", "View Tasks", "Edit Task", "Remove Task", "Stop Manager -- Not Implemented", "Restart Manager -- Not Implemented", "Exit" };
        
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
                    EditMenu();
                    break;
                case 3: // Remove Task
                    RemoveMenu();
                    break;
                case 4: // Stop Manager
                    break;
                case 5: // Restart Manager
                    break;
                case 6: // Exit
                    Environment.Exit(0); break;
                default: break;
            }
        }
    }

    static void EditMenu() {
        int[] selection = [0,0]; // Row, Column
        ConsoleKey key;

        // Check if the TaskList is empty
        if (GlobalData.TaskList.Count == 0) {

            // Clear and print title
            Console.Clear();
            Console.WriteLine("All Tasks\n(Press any key to go back)\n");

            Console.WriteLine("  No Tasks");
            Console.ReadKey();
            return;
        }

        while (true) {

            // Clear and print title
            Console.Clear();
            Console.WriteLine("All Tasks\n(Press ESC to go back)\n");

            // Display header row
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Task Name |    date     | Repeat? | Intval | Command ");
            Console.ResetColor();

            int printLine = 0;
            foreach (var item in GlobalData.TaskList) {
                //List<string> itemList = [item.TaskName, item.Date, item.Repeats.ToString(), item.RepeatInterval, item.Command];
                List<string> itemList = TaskToList(item, true);
            
                int column = 0;
                Console.Write(" ");
                foreach (string str in itemList) {
                    if (printLine == selection[0] && column == selection[1]) {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.Write($" {str} ");
                    Console.ResetColor();
                    if (column != 4) Console.Write("|");
                    column++;
                }
                Console.Write("\n");
            
                printLine++;
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.RightArrow && selection[1] != 4) selection[1]++;
            else if (key == ConsoleKey.LeftArrow && selection[1] != 0) selection[1]--;
            else if (key == ConsoleKey.UpArrow && selection[0] != 0) selection[0]--;
            else if (key == ConsoleKey.DownArrow && selection[0] != GlobalData.TaskList.Count-1) selection[0]++;
            else if (key == ConsoleKey.Escape) break;
            else if (key == ConsoleKey.Enter) editAttribute(selection[0], selection[1]);
        }
    }

    static void editAttribute(int taskListIndex, int itemIndex) {
        List<string> attrList = ["Task Name", "Date", "Task Repeat?", "Repeat Interval", "Command"];
        Console.Clear();
        Console.Write($"Input new data for ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{attrList[itemIndex]}");
        Console.ResetColor();
        Console.Write(" in task ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{GlobalData.TaskList[taskListIndex].TaskName}:\n");
        Console.ResetColor();

        Console.WriteLine("Type '!EXIT?' to go back");

        string inp;// = Console.ReadLine();
        bool invalidInput = true;


        while (invalidInput) {
            switch (itemIndex) {
                case 0: 
                    Console.WriteLine("Task Name: ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return;

                    if (TaskManager.ValidateTaskName(inp)) {
                        GlobalData.TaskList[taskListIndex].TaskName = inp;
                        invalidInput = false;
                    }
                    break;
                case 1:
                    Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return;
                    if (TaskManager.ValidateDate(inp, "MM/dd HH:mm")) {
                        GlobalData.TaskList[taskListIndex].Date = inp;
                        invalidInput = false;
                    }
                    break;
                case 2:
                    Console.WriteLine("Repeat Task? (Y/N): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return;
                    if (TaskManager.ValidateRepeatTask(inp)) {
                        if (inp == "Y") {
                            GlobalData.TaskList[taskListIndex].Repeats = true;
                            itemIndex++;
                        }
                        else {
                            GlobalData.TaskList[taskListIndex].Repeats = false;
                            GlobalData.TaskList[taskListIndex].RepeatInterval = null;
                            invalidInput = false;
                        }
                    }
                    break;
                case 3:
                    if (!GlobalData.TaskList[taskListIndex].Repeats) {
                        Console.Write("\nCannot change Repeat Interval if Repeating is off.\n(Press any key to go back)");
                        Console.ReadKey(true);
                        return;
                    }
                    Console.WriteLine("Repeat Interval (min, hr, day, week, mon, year): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") {
                        if (GlobalData.TaskList[taskListIndex].Repeats && GlobalData.TaskList[taskListIndex].RepeatInterval == null) {
                            Console.WriteLine("Cannot exit without specifiying a Repeat Interval");
                            break;
                        }
                        else return;
                    }
                    if (TaskManager.ValidateRepeatInterval(inp)) {
                        GlobalData.TaskList[taskListIndex].RepeatInterval = inp;
                        invalidInput = false;
                    }
                    break;
                case 4:
                    Console.WriteLine("Terminal Command: ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return;
                    if (!string.IsNullOrEmpty(inp)) {
                        GlobalData.TaskList[taskListIndex].Command = inp;
                        invalidInput = false;
                    }
                    else Console.Write("Cannot be empty. Try Again\n");
                    break;
            }
        }

        jsonHandler.SaveJsonData();
    }

    static List<string> TaskToList(Task item, bool fixedLength = false) {
        if (!fixedLength) return [item.TaskName, item.Date, item.Repeats.ToString(), item.RepeatInterval ?? "Null", item.Command];
        return [FixedLength(item.TaskName, 8), FixedLength(item.Date, 11), FixedLength(item.Repeats.ToString(), 7), FixedLength(item.RepeatInterval ?? "Null", 6), item.Command];
    }

    /// <summary>
    /// Displays all attributes for each saved task
    /// </summary>
    static void ViewTasks() {

        // Clear and print title
        Console.Clear();
        Console.WriteLine("All Tasks\n(Press any key to go back)\n");

        // Check if the TaskList is empty
        if (GlobalData.TaskList.Count == 0) {

            Console.WriteLine("  No Tasks");
            Console.ReadKey();
            return;
        }

        // Display header row
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" Task Name |    date     | Repeat? | Intval | Command ");
        Console.ResetColor();

        // Display each tasks attributes
        foreach (var item in GlobalData.TaskList) {

            Console.WriteLine($"  {FixedLength(item.TaskName, 8)} | {FixedLength(item.Date, 11)} | {FixedLength(item.Repeats.ToString(), 7)} | {FixedLength(item.RepeatInterval ?? "Null", 6)} | {item.Command}");
        }

        Console.ReadKey(); // Wait for any key press
    }

    /// <summary>
    /// Takes in any string and sets it to the specified length by trimming it or padding as needed.
    /// </summary>
    /// <param name="str">The string to be modified</param>
    /// <param name="len">The length to set the string to</param>
    /// <returns>A string at the specified length</returns>
    static string FixedLength(string str, int len) {

        if (str.Length > len) return str.Substring(0, len); // Trim string if its too long
        else if (str.Length < len) return str.PadRight(len); // Pad string if its too short
        return str; // Return without changing
    }

    /// <summary>
    /// The main 'home' menu to select different views
    /// </summary>
    /// <param name="options">List of views to choose from</param>
    /// <param name="prompt">Intro prompt, eg. 'Welcome to ...'</param>
    /// <returns>Int index of which option you choose; options[selected]</returns>
    static int ShowMenu(List<string> options, string prompt = "Choose an option:") {

        // Initilizing Variables
        int selected = 0;
        ConsoleKey key;

        // Update loop
        do {

            // Clear and print title
            Console.Clear();
            Console.WriteLine(prompt + "\n");

            // Display all options
            for (int i = 0; i < options.Count; i++) {

                if (i == selected) Console.ForegroundColor = ConsoleColor.Green; // Highlight selected option in green

                Console.WriteLine($"  {options[i]}");
                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key; // Read keyboard presses

            // Interpret keyboard presses
            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + options.Count) % options.Count;
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % options.Count;

        } while (key != ConsoleKey.Enter); // Loop while enter hasn't been pressed; nothing has been selected

        return selected; // Return options index
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