using System.Globalization;

namespace Managers;

/// <summary>
/// Manages how the global task list is manipulated
/// </summary>

public static class TaskManager
{
    private static DateTime parsedDate; // Dummy variable for CheckDate()

    ///
    ///     MANAGE TASKS
    /// 

    /// 0 = good, 1 = TaskList is null, 2 = TaskList.Date = null
    public static int NewTask()
    {
        if (GlobalData.TaskList == null) return 1;

        ScheduledTask item = new ScheduledTask(); // Create new task variable

        // Get new task name
        Console.Clear();
        Console.WriteLine("Task Name: ");
        item.TaskName = Console.ReadLine(); // Save the task name

        int nameStatus = ValidateTaskName(item.TaskName);
        while (nameStatus != 0)
        {
            if (nameStatus == 1) return 1;

            item.TaskName = Console.ReadLine();
            nameStatus = ValidateTaskName(item.TaskName);
        }

        // Get execution date
        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
        item.Date = Console.ReadLine();

        while (!ValidateDate(item.Date, "MM/dd HH:mm"))
        {

            item.Date = Console.ReadLine();
        }

        int day;
        int month;

        // Add a value for TrueDate to save the original day of a task if the day is scheduled at the end of a month
        if (item.Date != null)
        {
            day = int.Parse(item.Date.Substring(3, 3));
            month = int.Parse(item.Date.Substring(0, 1));
        }
        else return 2;

        if (day > 28) item.TrueDate = day;
        else if (day == 29 && month == 2) item.Date = $"02/28 {item.Date.Substring(6)}"; // No leap year

        // Does the task repeat?
        Console.WriteLine("Repeat Task? (Y/N): ");
        string? inp = Console.ReadLine();

        while (!ValidateRepeatTask(inp))
        {
            inp = Console.ReadLine();
        }

        // Set item repeat bool
        if (inp == "Y") item.Repeats = true;
        else item.Repeats = false;

        // If the task is repeating, how often?
        if (item.Repeats)
        {

            Console.WriteLine("Repeat Interval (min, hr, day, week, mon, year): ");

            item.RepeatInterval = Console.ReadLine();

            while (!ValidateRepeatInterval(item.RepeatInterval))
            {
                item.RepeatInterval = Console.ReadLine();
            }
        }
        else item.RepeatInterval = null; // Set the repeat interval to Null if the task doesn't repeat

        // Get command to execute
        Console.WriteLine("Terminal Command: ");
        item.Command = Console.ReadLine();

        // Check the the command is valid
        while (string.IsNullOrWhiteSpace(item.Command))
        {

            Console.WriteLine("Cannot be empty. Try Again: ");
            item.Command = Console.ReadLine();
        }

        GlobalData.TaskList.Add(item); // Add the new task to the global TaskList
        JsonHandler.SaveJsonData(); // Save Changes
        LogManager.frontLog.Information($"Created Task '{item.TaskName}'");
        return 0;
    }

    /// <summary>
    /// Removes a task from the global TaskList and save the change
    /// </summary>
    /// <param name="name">The TaskName of the task you want to remove</param>
    public static int RemoveItem(string name)
    {
        if (GlobalData.TaskList == null)
        {
            //ConsoleManager.ErrorScreen($"Cannot Remove Task '{name}'; TaskList is Null");
            return 1;
        }
        int index; // index of the item to be removed

        // Loop through all tasks until the task is found
        for (index = 0; index < GlobalData.TaskList.Count; index++)
        {
            if (GlobalData.TaskList[index].TaskName == name) break;
        }

        // // Return an error if no task was found
        if (index >= GlobalData.TaskList.Count) return 2;

        GlobalData.TaskList.RemoveAt(index); // Remove selected task
        JsonHandler.SaveJsonData(); // Save changes
        return 0;
    }

    public static bool EditAttribute(int taskListIndex, int itemIndex)
    {
        if (GlobalData.TaskList == null) return false;

        string? inp;
        bool invalidInput = true;

        while (invalidInput)
        {
            switch (itemIndex)
            {
                case 0:
                    Console.WriteLine("\nTask Name: ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return true;

                    int nameStatus = ValidateTaskName(inp);
                    if (nameStatus == 0)
                    {
                        GlobalData.TaskList[taskListIndex].TaskName = inp;
                        invalidInput = false;
                    }
                    else if (nameStatus == 1) return false;
                    break;
                case 1:
                    Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return true;
                    if (ValidateDate(inp, "MM/dd HH:mm"))
                    {
                        GlobalData.TaskList[taskListIndex].Date = inp;
                        invalidInput = false;
                    }
                    break;
                case 2:
                    Console.WriteLine("\nRepeat Task? (Y/N): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return true;
                    if (ValidateRepeatTask(inp))
                    {
                        if (inp == "Y")
                        {
                            GlobalData.TaskList[taskListIndex].Repeats = true;
                            itemIndex++;
                        }
                        else
                        {
                            GlobalData.TaskList[taskListIndex].Repeats = false;
                            GlobalData.TaskList[taskListIndex].RepeatInterval = null;
                            invalidInput = false;
                        }
                    }
                    break;
                case 3:
                    if (!GlobalData.TaskList[taskListIndex].Repeats)
                    {
                        Console.Write("\nCannot change Repeat Interval if Repeating is off.\n(Press any key to go back)");
                        Console.ReadKey(true);
                        return true;
                    }
                    Console.WriteLine("\nRepeat Interval (min, hr, day, week, mon, year): ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?")
                    {
                        if (GlobalData.TaskList[taskListIndex].Repeats && GlobalData.TaskList[taskListIndex].RepeatInterval == null)
                        {
                            Console.WriteLine("Cannot exit without specifiying a Repeat Interval");
                            break;
                        }
                        else return true;
                    }
                    if (ValidateRepeatInterval(inp))
                    {
                        GlobalData.TaskList[taskListIndex].RepeatInterval = inp;
                        invalidInput = false;
                    }
                    break;
                case 4:
                    Console.WriteLine("\nTerminal Command: ");
                    inp = Console.ReadLine();
                    if (inp == "!EXIT?") return true;
                    if (!string.IsNullOrEmpty(inp))
                    {
                        GlobalData.TaskList[taskListIndex].Command = inp;
                        invalidInput = false;
                    }
                    else Console.Write("Cannot be empty. Try Again\n");
                    break;
            }
        }
        return true;
    }

    ///
    ///     VALIDATING TASK INFO
    /// 

    public static int ValidateTaskName(string? name)
    {
        if (GlobalData.TaskList == null) return 1;
        // Is the name empty?
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be empty. Try Again");
            return 2; // Reset the loop and check again
        }

        // Make sure name isn't already in use
        foreach (ScheduledTask item in GlobalData.TaskList)
        {
            if (item.TaskName == name)
            { // Compare all task names, return false if there is a match
                Console.WriteLine("Name already in use. Try Again");
                return 3;
            }
        }

        return 0; // Name is valid
    }

    public static bool ValidateDate(string? date, string format)
    {

        if (string.IsNullOrWhiteSpace(date))
        { // null or blank

            Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            return false;
        }

        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        { // invlaid format
            Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");
            return false;
        }

        return true; // correct format

        //if (DateCheckStatus == 1) Console.WriteLine("Cannot be empty. Try Again: ");
        //if (DateCheckStatus == 2) Console.WriteLine("Invalid Format (MM/dd HH:mm), eg. (03/13 15:00)\nTry again: ");

    }

    public static bool ValidateRepeatTask(string? inp)
    {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToUpper() != "Y" && inp.ToUpper() != "N"))
        {

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true;
    }

    public static bool ValidateRepeatInterval(string? inp)
    {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToLower() != "min" && inp.ToLower() != "hr" && inp.ToLower() != "day" && inp.ToLower() != "week" && inp.ToLower() != "mon" && inp.ToLower() != "year"))
        {

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true;
    }


    ///
    ///     GETTING TASK INFO
    /// 


    /// <summary>
    /// Gets a List of all task names
    /// </summary>
    /// <returns>Returns a string list with all task names</returns>
    public static List<string>? GetTaskNames()
    {
        if (GlobalData.TaskList == null) return null;

        List<string> names = []; // Create empty name list
        foreach (var item in GlobalData.TaskList)
        {
            if (item.TaskName != null) names.Add(item.TaskName); // Populate list with task names
        }

        return names; // return task name list
    }

    public static List<string> TaskToList(ScheduledTask item, bool fixedLength = false)
    {
        if (!fixedLength) return [item.TaskName, item.Date, item.Repeats.ToString(), item.RepeatInterval ?? "Null", item.Command];
        return [FixedLength(item.TaskName, 8), FixedLength(item.Date, 11), FixedLength(item.Repeats.ToString(), 7), FixedLength(item.RepeatInterval ?? "Null", 6), item.Command];
    }

    public static string FixedLength(string? str, int len) {

        if (str == null) return "";
        else if (str.Length > len) return str.Substring(0, len); // Trim string if its too long
        else if (str.Length < len) return str.PadRight(len); // Pad string if its too short
        return str; // Return without changing
    }
}