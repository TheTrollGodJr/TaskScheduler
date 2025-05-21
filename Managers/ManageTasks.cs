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
    

    ///<summary>
    ///  Gets user input to create a new ScheduledTask
    ///</summary>
    /// <returns>Int status of task creation. 0 = Successful, 1 = TaskList is null, 2 = User input date is null, 3 = Error saving data</returns>
    public static int NewTask()
    {
        if (GlobalData.TaskList == null) return 1;

        ScheduledTask item = new ScheduledTask(); // Create new task variable

        // Get new task name
        Console.Clear();
        Console.WriteLine("Task Name: ");
        item.TaskName = Console.ReadLine(); // Save the task name

        // Check task name
        int nameStatus = ValidateTaskName(item.TaskName);
        while (nameStatus != 0)
        {   // Task name invalid or error during validating

            if (nameStatus == 1) return 1; // TaskList is null

            // Get and check new task name
            item.TaskName = Console.ReadLine();
            nameStatus = ValidateTaskName(item.TaskName);
        }

        // Get execution date
        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
        item.Date = Console.ReadLine();

        // Check date
        while (!ValidateDate(item.Date, "MM/dd HH:mm")) item.Date = Console.ReadLine();

        // Declaring date variables
        int day;
        int month;

        // Add a value for TrueDate to save the original day of a task if the day is scheduled at the end of a month
        if (item.Date != null)
        {

            // Initilizing date variables
            day = int.Parse(item.Date.Substring(3, 3));
            month = int.Parse(item.Date.Substring(0, 1));
        }
        else return 2; // Date is null

        // Set the truth day of a date for months to revert back to after months with less days
        if (day > 28) item.TrueDate = day;
        else if (day == 29 && month == 2) item.Date = $"02/28 {item.Date.Substring(6)}"; // Account for leap year

        // Does the task repeat?
        Console.WriteLine("Repeat Task? (Y/N): ");
        string? inp = Console.ReadLine();

        // Check user input
        while (!ValidateRepeatTask(inp)) inp = Console.ReadLine();

        // Set item repeat bool
        if (inp == "Y") item.Repeats = true;
        else item.Repeats = false;

        // If the task is repeating, how often?
        if (item.Repeats)
        {

            Console.WriteLine("Repeat Interval (min, hr, day, week, mon, year): ");
            item.RepeatInterval = Console.ReadLine();

            // Check repeat interval
            while (!ValidateRepeatInterval(item.RepeatInterval)) item.RepeatInterval = Console.ReadLine();
        }
        else item.RepeatInterval = null; // Set the repeat interval to Null if the task doesn't repeat

        // Get command to execute
        Console.WriteLine("Terminal Command: ");
        item.Command = Console.ReadLine();

        // Check the the command is valid
        while (string.IsNullOrWhiteSpace(item.Command))
        {   // Command is invalid

            Console.WriteLine("Cannot be empty. Try Again: ");
            item.Command = Console.ReadLine();
        }

        GlobalData.TaskList.Add(item); // Add the new task to the global TaskList

        if (!JsonHandler.SaveJsonData()) // Save Changes
        {   // Saving Failed

            LogManager.log.Error($"Failed to Save TaskList With New Task '{item.TaskName}'");
            return 3;
        }

        LogManager.frontLog.Information($"Created Task '{item.TaskName}'");
        return 0; // Task creation successful
    }

    /// <summary>
    /// Removes a task from the global TaskList and save the change
    /// </summary>
    /// <param name="name">The TaskName of the task you want to remove</param>
    /// <returns>Int status of removal. 0 = successful, 1 = TaskList is null, 2 = Task could not be found, 3 = Error saving TaskList data</returns>
    public static int RemoveItem(string name)
    {
        if (GlobalData.TaskList == null) return 1; // TaskList is null

        int index; // index of the item to be removed

        // Loop through all tasks until the task is found
        for (index = 0; index < GlobalData.TaskList.Count; index++) if (GlobalData.TaskList[index].TaskName == name) break;

        // // Return an error if no task was found
        if (index >= GlobalData.TaskList.Count) return 2;

        GlobalData.TaskList.RemoveAt(index); // Remove selected task

        if (!JsonHandler.SaveJsonData()) // Save changes
        {   // Saving failed

            LogManager.log.Error($"Failed to Save TaskList With Removed Task '{name}'");
            return 3;
        }

        return 0;
    }

    /// <summary>
    /// Edits the value of a specified value in a ScheduledTask task
    /// </summary>
    /// <param name="taskListIndex">The Task index of TaskList to edit</param>
    /// <param name="itemIndex">The index of the Task attribute to edit: [TaskName=0, Date=1, TaskRepeat?=2, RepeatInterval=3, Command=4]</param>
    /// <returns>True if the attribute was edited successfully, false if it wasn't</returns>
    public static bool EditAttribute(int taskListIndex, int itemIndex)
    {
        if (GlobalData.TaskList == null) return false; // Couldn't get TaskList

        string? inp; // User input
        bool invalidInput = true;

        try // Try to edit the attribute
        {

            while (invalidInput) // Keep prompting the user for input until their input is valid
            {

                switch (itemIndex) // Which attribute to edit
                {

                    case 0:

                        // Get input
                        Console.WriteLine("\nTask Name: ");
                        inp = Console.ReadLine();

                        if (inp == "!EXIT?") return true; // Go back

                        // Validate and check input
                        int nameStatus = ValidateTaskName(inp); // Check input
                        if (nameStatus == 0)
                        { // Input was valid

                            GlobalData.TaskList[taskListIndex].TaskName = inp;
                            invalidInput = false; // Break loop
                        }
                        else if (nameStatus == 1) return false; // TaskList is null
                        break;

                    case 1:

                        // Get input
                        Console.WriteLine("Schedule Date (MM/dd HH:mm): ");
                        inp = Console.ReadLine();

                        if (inp == "!EXIT?") return true; // Go back

                        // Check date
                        if (ValidateDate(inp, "MM/dd HH:mm"))
                        { // Date is valid

                            GlobalData.TaskList[taskListIndex].Date = inp;
                            invalidInput = false; // Break loop
                        }
                        break;

                    case 2:

                        // Get input
                        Console.WriteLine("\nRepeat Task? (Y/N): ");
                        inp = Console.ReadLine();

                        if (inp == "!EXIT?") return true; // Go back

                        // Check input
                        if (ValidateRepeatTask(inp))
                        { // Input is valid

                            if (inp == "Y")
                            { // Save selection "Y" as true

                                GlobalData.TaskList[taskListIndex].Repeats = true;
                                itemIndex++; // Move edit selection to select a repeat interval
                            }
                            else
                            { // Save selection "N" as false

                                GlobalData.TaskList[taskListIndex].Repeats = false;
                                GlobalData.TaskList[taskListIndex].RepeatInterval = null;
                                invalidInput = false; // Break loop
                            }
                        }
                        break;

                    case 3:

                        // This attribute can only be changed if case 2 is set to true
                        // Check if this attribute can be edited
                        if (!GlobalData.TaskList[taskListIndex].Repeats)
                        {   // Cannot edit this attribute

                            Console.Write("\nCannot change Repeat Interval if Repeating is off.\n(Press any key to go back)");
                            Console.ReadKey(true);
                            return true; // Exit function
                        }

                        // Get input
                        Console.WriteLine("\nRepeat Interval (min, hr, day, week, mon, year): ");
                        inp = Console.ReadLine();

                        if (inp == "!EXIT?") // Go back
                        {

                            // Cannot exit if no repeat interval is specified
                            if (GlobalData.TaskList[taskListIndex].Repeats && GlobalData.TaskList[taskListIndex].RepeatInterval == null)
                            {

                                Console.WriteLine("Cannot exit without specifiying a Repeat Interval");
                                break;
                            }

                            else return true; // Exit function
                        }

                        // Check input
                        if (ValidateRepeatInterval(inp))
                        { // Input is valid

                            GlobalData.TaskList[taskListIndex].RepeatInterval = inp;
                            invalidInput = false; // Break loop
                        }
                        break;

                    case 4:

                        // Get input
                        Console.WriteLine("\nTerminal Command: ");
                        inp = Console.ReadLine();

                        if (inp == "!EXIT?") return true; // Go back

                        // Check input
                        if (!string.IsNullOrEmpty(inp))
                        { // Input is valid

                            GlobalData.TaskList[taskListIndex].Command = inp;
                            invalidInput = false; // Break loop
                        }
                        else Console.Write("Cannot be empty. Try Again\n");
                        break;
                }
            }
        }
        catch
        {
            return false; // Error occured when editing
        }

        return true; // Edit successful
    }

    static string UpdateDate(string date, string interval, int? trueDate = null) {
        switch (interval) {
            case "min": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMinutes(1).ToString("MM/dd HH:mm");
            case "hr": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddHours(1).ToString("MM/dd HH:mm");
            case "mon":
                if (trueDate != null)
                {
                    DateTime nextMonth = DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMonths(1);
                    int lastDay = DateTime.DaysInMonth(2024, nextMonth.Month);
                    int day = Math.Min(trueDate.Value, lastDay);
                    string newDate = new DateTime(2024, nextMonth.Month, day, nextMonth.Hour, nextMonth.Minute, 0).ToString("MM/dd HH:mm");
                    if (newDate.Contains("02/29")) return $"02/28 {newDate.Substring(6)}";
                    return newDate;
                }
                else return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMonths(1).ToString("MM/dd HH:mm");
            case "week": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddDays(7).ToString("MM/dd HH:mm");
            case "day": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddDays(1).ToString("MM/dd HH:mm");
            case "year": return date;
        }
        return date;
    }

    public static void UpdateRepeatTime(string taskName) {
        if (GlobalData.TaskList == null)
        {
            LogManager.log.Error("Cannot Update Repeat Time; TaskList is Null");
            return;
        }
        
        foreach (var item in GlobalData.TaskList)
        {
            if (item.TaskName == taskName)
            {
                if (item.Repeats && item.Date != null && item.RepeatInterval != null)
                {
                    LogManager.log.Information($"Updating Repeat Interval For Task '{taskName}'");
                    item.Date = UpdateDate(item.Date, item.RepeatInterval, item.TrueDate);
                    if (!JsonHandler.SaveJsonData()) LogManager.log.Error("Failed to Save Updated RepeatInterval Data to tasks.json");
                    break;
                }
                else
                {
                    LogManager.log.Information($"Removing Task '{taskName}'");
                    RemoveItem(taskName);
                    break;
                }
            }
        }
    }


    ///
    ///     VALIDATING TASK INFO
    /// 


    /// <summary>
    /// Checks a string to ensure it is a valid name for a ScheduledTask variable.
    /// </summary>
    /// <param name="name">String to validate</param>
    /// <returns>Int value status. 0 indicates the name is valid. 
    /// 1 indicates that the TaskList is null. 
    /// 2 indicates that the name parameters is null.
    /// 3 indicates that the namem is already in use.
    /// </returns>
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

/// <summary>
/// Ensures that the date parameters is a valid date
/// </summary>
/// <param name="date">Date to be verified</param>
/// <param name="format">The format that the date parameters is in</param>
/// <returns>True if the date is valid, false if it is not</returns>
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

/// <summary>
/// Ensures that the string parameters is either "Y" or "N"
/// </summary>
/// <param name="inp">String to be verified</param>
/// <returns>True if the string parameter is valid, false if it is not</returns>
    public static bool ValidateRepeatTask(string? inp)
    {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToUpper() != "Y" && inp.ToUpper() != "N"))
        { // inp is not valid

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true; // inp is valid
    }

/// <summary>
/// Ensures that the string parameter is a valid repeat interval: min, hr, day, week, mon, or year
/// </summary>
/// <param name="inp">String parameter to be verified</param>
/// <returns>True if the string parameter is valid, false if it is not</returns>
    public static bool ValidateRepeatInterval(string? inp)
    {

        if (string.IsNullOrWhiteSpace(inp) || (inp.ToLower() != "min" && inp.ToLower() != "hr" && inp.ToLower() != "day" && inp.ToLower() != "week" && inp.ToLower() != "mon" && inp.ToLower() != "year"))
        { // imp is not valid

            Console.WriteLine("Invalid Input. Try Again: ");
            return false;
        }

        return true; // imp is valid
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
        if (GlobalData.TaskList == null) return null; // Couldn't get TaskList

        List<string> names = []; // Create empty name list
        foreach (var item in GlobalData.TaskList)
        {
            if (item.TaskName != null) names.Add(item.TaskName); // Populate list with task names
        }

        return names; // return task name list
    }

    /// <summary>
    /// Takes a variable with the type ScheduledTask and converts all variable data into a string array
    /// </summary>
    /// <param name="item">The ScheduledTask variable to extract data from</param>
    /// <param name="fixedLength">Bool to trim strings in the output; set to false by default</param>
    /// <returns>String list with all ScheduledTask data.</returns>
    public static List<string> TaskToList(ScheduledTask item, bool fixedLength = false)
    {
        if (!fixedLength) return [item.TaskName, item.Date, item.Repeats.ToString(), item.RepeatInterval ?? "Null", item.Command];
        return [FixedLength(item.TaskName, 8), FixedLength(item.Date, 11), FixedLength(item.Repeats.ToString(), 7), FixedLength(item.RepeatInterval ?? "Null", 6), item.Command];
    }

    /// <summary>
    /// Trim or pad a string to a sepcificed length
    /// </summary>
    /// <param name="str">String variable to be trimmed or padded</param>
    /// <param name="len">Length of the string output</param>
    /// <returns>A string of the specified length</returns>
    public static string FixedLength(string? str, int len)
    {

        if (str == null) return "";
        else if (str.Length > len) return str.Substring(0, len); // Trim string if its too long
        else if (str.Length < len) return str.PadRight(len); // Pad string if its too short
        return str; // Return without changing
    }
}