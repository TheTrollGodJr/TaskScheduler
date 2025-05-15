using System.Globalization;
using Managers;
// using Shared;

namespace Managers;

/// <summary>
/// Manages how the global task list is manipulated
/// </summary>

public static class TaskManager {
    private static DateTime parsedDate; // Dummy variable for CheckDate()

    /// <summary>
    /// Creates a new Task using user input and saves it
    /// </summary>
    public static void NewTask() {
        ScheduledTask item = new ScheduledTask(); // Create new task variable

        // Get new task name
        Console.Clear();
        Console.WriteLine("Task Name: ");
        item.TaskName = Console.ReadLine(); // Save the task name

        while (!ValidateTaskName(item.TaskName)) {
            item.TaskName = Console.ReadLine();
        }

        // Get execution date
        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
        item.Date = Console.ReadLine(); 
        
        Console.WriteLine("Checking Date");
        while (!ValidateDate(item.Date, "MM/dd HH:mm")) { /// ------------------ PROGRAM FAILS HERE AFTER SUCCESSFULLY RUNNING ValidateDate()
            Console.WriteLine("Valid");
            item.Date = Console.ReadLine();
            Console.WriteLine("Date Set");
        }

        // Add a value for TrueDate to save the original day of a task if the day is scheduled at the end of a month
        int day = int.Parse(item.Date.Substring(3, 4));
        Console.WriteLine("Got day");
        int month = int.Parse(item.Date.Substring(0, 1));
        Console.WriteLine("Got month");
        //Console.WriteLine($"Day: {day}\nMon: {month}");
        if (day > 28) item.TrueDate = day;
        else if (day == 29 && month == 2) item.Date = $"02/28 {item.Date.Substring(6)}"; // No leap year
        Console.WriteLine("Fixed exceptions");

        // Does the task repeat?
        Console.WriteLine("Repeat Task? (Y/N): ");
        string inp = Console.ReadLine();

        while (!ValidateRepeatTask(inp)) {
            inp = Console.ReadLine();
        }

        // Set item repeat bool
        if (inp == "Y") item.Repeats = true;
        else item.Repeats = false;

        // If the task is repeating, how often?
        if (item.Repeats) {

            Console.WriteLine("Repeat Interval (min, hr, day, week, mon, year): ");
            //inp = Console.ReadLine();
            item.RepeatInterval = Console.ReadLine();

            while (!ValidateRepeatInterval(item.RepeatInterval)) {
                item.RepeatInterval = Console.ReadLine();
            }
        }
        else item.RepeatInterval = null; // Set the repeat interval to Null if the task doesn't repeat

        // Get command to execute
        Console.WriteLine("Terminal Command: ");
        item.Command = Console.ReadLine();

        // Check the the command is valid
        while (string.IsNullOrWhiteSpace(item.Command)) {

            Console.WriteLine("Cannot be empty. Try Again: ");
            item.Command = Console.ReadLine();
        }

        GlobalData.TaskList.Add(item); // Add the new task to the global TaskList
        JsonHandler.SaveJsonData(); // Save Changes
    }

    public static bool ValidateTaskName(string name) {
        // Is the name empty?
        if (string.IsNullOrWhiteSpace(name)) {
            Console.WriteLine("Name cannot be empty. Try Again");
            return false; // Reset the loop and check again
        }
        
        // Make sure name isn't already in use
        foreach (ScheduledTask item in GlobalData.TaskList) {
            if (item.TaskName == name) { // Compare all task names, return false if there is a match
                Console.WriteLine("Name already in use. Try Again");
                return false; 
            }
        }

        return true; // Name is valid
    }

    /// <summary>
    /// Checks if the inputed date was inputed correctly/is valid to the specified format
    /// </summary>
    /// <param name="date">The date to check</param>
    /// <param name="format">The format the date should be in.</param>
    /// <returns>
    /// returns the status of the date
    /// 0 - Valid, 1 - String is empty, 2 - Invalid time format
    /// </returns>
    /*
    static int CheckDate(string date, string format) {

        if (string.IsNullOrWhiteSpace(date)) return 1; // null or blank
        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) return 2; // invlaid format
        return 0; // correct format
    }
    */

    public static bool ValidateDate(string date, string format) {
        Console.WriteLine("Running ValidateDate()");
        if (string.IsNullOrWhiteSpace(date)) { // null or blank

            Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            return false;
        } 

        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) { // invlaid format
            Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            return false;
        }
        Console.WriteLine("Valid Date");
        return true; // correct format

        //if (DateCheckStatus == 1) Console.WriteLine("Cannot be empty. Try Again: ");
        //if (DateCheckStatus == 2) Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            
    }

    public static bool ValidateRepeatTask(string inp) {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToUpper() != "Y" && inp.ToUpper() != "N")) {

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true;
    }

    public static bool ValidateRepeatInterval(string inp) {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToLower() != "min" && inp.ToLower() != "hr" && inp.ToLower() != "day" && inp.ToLower() != "week" && inp.ToLower() != "mon" && inp.ToLower() != "year")) {

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks that a task name is not already in use -- used when creating a new task
    /// </summary>
    /// <param name="name">The task name to check</param>
    /// <returns>True if the name is avaliable, False if it isn't</returns>
    /*
    static bool VerifyTaskName(string name) {
        
        // Loop through all tasks
        foreach (Task item in GlobalData.TaskList) {
            if (item.TaskName == name) return false; // Compare all task names, return false if there is a match
        }

        return true; // Name is valid
    }
    */

    /// <summary>
    /// Removes a task from the global TaskList and save the change
    /// </summary>
    /// <param name="name">The TaskName of the task you want to remove</param>
    public static void RemoveItem(string name) {

        int index; // index of the item to be removed

        // Loop through all tasks until the task is found
        for (index = 0; index < GlobalData.TaskList.Count; index++) {
            if (GlobalData.TaskList[index].TaskName == name) break;
        }

        // Do nothing if the task wasn't found
        if (index >= GlobalData.TaskList.Count) return;

        GlobalData.TaskList.RemoveAt(index); // Remove selected task
        JsonHandler.SaveJsonData(); // Save changes
    }

    /// <summary>
    /// Gets a List of all task names
    /// </summary>
    /// <returns>Returns a string list with all task names</returns>
    public static List<string> GetTaskNames() {

        List<string> names = []; // Create empty name list
        foreach (var item in GlobalData.TaskList) names.Add(item.TaskName); // Populate list with task names
        
        return names; // return task name list
    }
}