using System.Globalization;

namespace Managers.TaskManager;

/// <summary>
/// Manages how the global task list is manipulated
/// </summary>

public static class TaskManager {
    private static DateTime parsedDate; // Dummy variable for CheckDate()

    /// <summary>
    /// Creates a new Task using user input and saves it
    /// </summary>
    public static void NewTask() {
        Task item = new Task(); // Create new task variable

        // Get new task name
        Console.Clear();
        Console.WriteLine("Task Name: ");
        item.TaskName = Console.ReadLine(); // Save the task name

        // Check that the saved name if valid
        while (true) {

            // Is the name empty?
            if (string.IsNullOrWhiteSpace(item.TaskName)) {
                Console.WriteLine("Name cannot be empty. Try Again: ");
                item.TaskName = Console.ReadLine();
                continue; // Reset the loop and check again
            }

            // Is the name already taken?
            if (!VerifyTaskName(item.TaskName)) { 
                Console.WriteLine("That name is already in use. Try Again: ");
                item.TaskName = Console.ReadLine();
                continue; // Reset the loop and check again
            }

            break; // Break the loop; the name is valid
        }

        // Get execution date
        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
        item.Date = Console.ReadLine(); 
        int DateCheckStatus = CheckDate(item.Date, "MM/dd HH:mm"); // 0 - valid, 1 - empty string, 2 - invalid format

        // While the date isn't valid
        while (DateCheckStatus != 0) {

            if (DateCheckStatus == 1) Console.WriteLine("Cannot be empty. Try Again: ");
            if (DateCheckStatus == 2) Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            
            item.Date = Console.ReadLine();
            DateCheckStatus = CheckDate(item.Date, "MM/dd HH:mm"); // Re-Check the date
        }   

        // Does the task repeat?
        Console.WriteLine("Repeat Task? (Y/N): ");
        string inp = Console.ReadLine();

        // Check that the input is valid; Y/N and not empty
        while (string.IsNullOrWhiteSpace(inp) || (inp.ToUpper() != "Y" && inp.ToUpper() != "N")) {

            Console.WriteLine("Invalid Input. Try Again: ");
            inp = Console.ReadLine();
        }

        // Set item repeat bool
        if (inp == "Y") item.Repeats = true;
        else item.Repeats = false;

        // If the task is repeating, how often?
        if (item.Repeats) {

            Console.WriteLine("Repeat Interval (min, hr, day, week, mon, year): ");
            inp = Console.ReadLine();

            // Make sure the input is valid
            while (string.IsNullOrWhiteSpace(inp) || (inp.ToLower() != "min" && inp.ToLower() != "hr" && inp.ToLower() != "day" && inp.ToLower() != "week" && inp.ToLower() != "mon" && inp.ToLower() != "year")) {
                Console.WriteLine("Invalid Input. Try Again: ");
                inp = Console.ReadLine();
            }

            item.RepeatInterval = inp; // Set the repeat interval
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
        jsonHandler.jsonHandler.SaveJsonData(); // Save Changes
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
    static int CheckDate(string date, string format) {

        if (string.IsNullOrWhiteSpace(date)) return 1; // null or blank
        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) return 2; // invlaid format
        return 0; // correct format
    }

    /// <summary>
    /// Checks that a task name is not already in use -- used when creating a new task
    /// </summary>
    /// <param name="name">The task name to check</param>
    /// <returns>True if the name is avaliable, False if it isn't</returns>
    static bool VerifyTaskName(string name) {
        
        // Loop through all tasks
        foreach (Task item in GlobalData.TaskList) {
            if (item.TaskName == name) return false; // Compare all task names, return false if there is a match
        }

        return true; // Name is valid
    }

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
        jsonHandler.jsonHandler.SaveJsonData(); // Save changes
    }
}