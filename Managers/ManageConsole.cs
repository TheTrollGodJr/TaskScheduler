namespace Managers;

public static class ConsoleManager
{

    ///
    ///     MENU SELECTION FUNCTIONS
    /// 


    /// <summary>
    /// 
    /// </summary>
    public static void LaunchConsoleApp()
    {
        JsonHandler.UnlockJson(GlobalData.lockPath);

        LogManager.frontLog.Information("Started Frontend Console Process");

        var options = new List<string> { "Create Task", "View Tasks", "Edit Task", "Remove Task", "Stop Manager -- Not Implemented", "Restart Manager -- Not Implemented", "Exit" };

        while (true)
        {
            int choice = ShowMenu(options, "Welcome to Task Scheduler");

            //Console.Clear();
            switch (choice)
            {
                case 0: // Add Task
                    int status = TaskManager.NewTask(); // 0 = good, 1 = TaskList is null, 2 = TaskList.Date = null
                    if (status == 1) ErrorScreen("Could Not Create New Task; TaskList is Null");
                    else if (status == 2) ErrorScreen("Could Not Create New Task; TaskList Task.Date is Null");
                    else if (status == 3) ErrorScreen("Could Not Save New Task");
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
                    LogManager.frontLog.Information("Closed Console Frontend");
                    Environment.Exit(0);
                    break;
                default: break;
            }
        }
    }


    static void EditMenu()
    {
        if (GlobalData.TaskList == null)
        {
            ErrorScreen("Could Not Edit TaskList; TaskList is Null");
            return;
        }

        int[] selection = [0, 0]; // Row, Column
        ConsoleKey key;

        // Check if the TaskList is empty
        if ((GlobalData.TaskList?.Count ?? 0) == 0)
        {

            // Clear and print title
            Console.Clear();
            Console.WriteLine("All Tasks\n(Press any key to go back)\n");

            Console.WriteLine("  No Tasks");
            Console.ReadKey();
            return;
        }

        while (true)
        {

            // Clear and print title
            Console.Clear();
            Console.WriteLine("All Tasks\n(Press ESC to go back)\n");

            // Display header row
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Task Name |    date     | Repeat? | Intval | Command ");
            Console.ResetColor();

            int printLine = 0;
            if (GlobalData.TaskList != null)
            {
                foreach (var item in GlobalData.TaskList)
                {
                    //List<string> itemList = [item.TaskName, item.Date, item.Repeats.ToString(), item.RepeatInterval, item.Command];
                    List<string> itemList = TaskManager.TaskToList(item, true);

                    int column = 0;
                    Console.Write(" ");
                    foreach (string str in itemList)
                    {
                        if (printLine == selection[0] && column == selection[1])
                        {
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
            }
            else
            {
                ErrorScreen("Could Not Edit TaskList; TaskList is Null");
                return;
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.RightArrow && selection[1] != 4) selection[1]++;
            else if (key == ConsoleKey.LeftArrow && selection[1] != 0) selection[1]--;
            else if (key == ConsoleKey.UpArrow && selection[0] != 0) selection[0]--;
            else if (key == ConsoleKey.DownArrow && selection[0] != GlobalData.TaskList.Count - 1) selection[0]++;
            else if (key == ConsoleKey.Escape) break;
            else if (key == ConsoleKey.Enter)
            {
                int editStatus = EditAttributeMenu(selection[0], selection[1]);
                if (editStatus == 1) ErrorScreen("Could Not Edit Tasks; TaskList is Null");
                else if (editStatus == 2) ErrorScreen($"Failed to Edit Attribute From Task '{GlobalData.TaskList[selection[0]].TaskName}'");
                else if (editStatus == 3) ErrorScreen($"Failed to Save Changes for Task '{GlobalData.TaskList[selection[0]].TaskName}'");
            }
        }
    }

    static int EditAttributeMenu(int taskListIndex, int itemIndex)
    {
        List<string> attrList = ["Task Name", "Date", "Task Repeat?", "Repeat Interval", "Command"];
        if (GlobalData.TaskList != null)
        {
            Console.Clear();
            Console.Write($"Input new data for ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{attrList[itemIndex]}");
            Console.ResetColor();
            Console.Write(" in task ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{GlobalData.TaskList[taskListIndex].TaskName}:\n");
            Console.ResetColor();
        }
        else
        {
            ErrorScreen("Could Not Edit TaskList; TaskList is Null");
            return 1;
        }

        Console.WriteLine("Type '!EXIT?' to go back");

        if (!TaskManager.EditAttribute(taskListIndex, itemIndex))
        {
            LogManager.frontLog.Error($"Failed to Edit Attribute '{attrList[itemIndex]}' From Task '{GlobalData.TaskList[taskListIndex].TaskName}'");
            return 2;
        }

        if (!JsonHandler.SaveJsonData()) // Save Changes
        {   // Failed to save changes

            LogManager.frontLog.Error($"Failed to Save Changes for Attribute '{attrList[itemIndex]}' From Task '{GlobalData.TaskList[taskListIndex].TaskName}'");
            return 3;
        }

        LogManager.frontLog.Information($"Edited Attribute '{attrList[itemIndex]}' from Task {GlobalData.TaskList[taskListIndex].TaskName}");
        return 0;
    }



    /// <summary>
    /// Displays all attributes for each saved task
    /// </summary>
    static void ViewTasks()
    {

        if (GlobalData.TaskList == null)
        {
            ErrorScreen("Could Not View TaskList; TaskList is Null");
            return;
        }

        // Clear and print title
        Console.Clear();
        Console.WriteLine("All Tasks\n(Press any key to go back)\n");

        // Check if the TaskList is empty
        if (GlobalData.TaskList.Count == 0)
        {

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
        foreach (var item in GlobalData.TaskList)
        {

            Console.WriteLine($"  {TaskManager.FixedLength(item.TaskName, 8)} | {TaskManager.FixedLength(item.Date, 11)} | {TaskManager.FixedLength(item.Repeats.ToString(), 7)} | {TaskManager.FixedLength(item.RepeatInterval ?? "Null", 6)} | {item.Command}");
        }

        Console.ReadKey(); // Wait for any key press
    }

    /// <summary>
    /// Takes in any string and sets it to the specified length by trimming it or padding as needed.
    /// </summary>
    /// <param name="str">The string to be modified</param>
    /// <param name="len">The length to set the string to</param>
    /// <returns>A string at the specified length</returns>


    /// <summary>
    /// The main 'home' menu to select different views
    /// </summary>
    /// <param name="options">List of views to choose from</param>
    /// <param name="prompt">Intro prompt, eg. 'Welcome to ...'</param>
    /// <returns>Int index of which option you choose; options[selected]</returns>
    static int ShowMenu(List<string> options, string prompt = "Choose an option:")
    {

        // Initilizing Variables
        int selected = 0;
        ConsoleKey key;

        // Update loop
        do
        {
            Thread.Sleep(10);
            // Clear and print title
            Console.Clear();
            Console.WriteLine(prompt + "\n");

            // Display all options
            for (int i = 0; i < options.Count; i++)
            {

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
    static void RemoveMenu()
    {
        // Initialize variables
        List<string>? taskNames = TaskManager.GetTaskNames(); // Get task names
        ConsoleKey key; // Console key object
        int selected = 0; // Selected task index

        if (taskNames == null)
        {
            ErrorScreen("Could Not Open RemoveMenu; TaskList is Null");
            return;
        }

        // If there are no saved tasks
        if (taskNames.Count == 0)
        {

            // Display message then wait for any key to return to the main menu
            Console.Clear();
            Console.WriteLine("Select a Task to remove\n(Press any key to go back)\n\n  No Tasks");
            Console.ReadKey();
            return;
        }

        // Show selection menu to select a task to remove
        do
        {

            Console.Clear();
            Console.WriteLine("Select a Task to remove\n(Press ESC to go back)\n");

            // Show all task names
            for (int i = 0; i < taskNames.Count; i++)
            {

                if (i == selected) Console.ForegroundColor = ConsoleColor.Green; // Highlight selected task in green

                Console.WriteLine($"  {taskNames[i]}");
                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key; // Get current key

            // Movement options using keyboard
            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + taskNames.Count) % taskNames.Count; // Select up
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % taskNames.Count; // Select down
            else if (key == ConsoleKey.Enter)
            { // Confirm deletion selection

                if (RemoveConfirmation(taskNames[selected]))
                { // Run function to confirm deletion

                    // Remove task
                    int removeStatus = TaskManager.RemoveItem(taskNames[selected]);
                    if (removeStatus == 1) ErrorScreen($"Cannot Remove Task '{taskNames[selected]}'; TaskList is Null");
                    else if (removeStatus == 2) ErrorScreen($"Cannot Remove Task '{taskNames[selected]}'; Task Cannot be Found");
                    else
                    {
                        taskNames.RemoveAt(selected);
                        LogManager.frontLog.Information($"Removed Task '{taskNames[selected]}'");
                    }
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
    static bool RemoveConfirmation(string name)
    {

        // Reset and write to the console
        Console.Clear();
        Console.WriteLine($"Are you sure you want to delete task: {name}? (YES/NO)");

        string? inp = Console.ReadLine();//.ToUpper(); // Get user input

        // Make sure the input is valid; either YES or NO
        while (inp == null || (inp.ToUpper() != "YES" && inp.ToUpper() != "NO"))
        {

            Console.WriteLine("Invalid Input. Try Again (YES/NO)");
            inp = Console.ReadLine();
        }

        // Return true if YES, return false if NO
        if (inp == "YES") return true;
        return false;
    }

    static void ErrorScreen(string message)
    {
        LogManager.frontLog.Error($"{message}");
        Console.Clear();
        Console.WriteLine($"Error:\n    {message}\n\nPress Enter To Exit");
        Console.ReadLine();
    }

}