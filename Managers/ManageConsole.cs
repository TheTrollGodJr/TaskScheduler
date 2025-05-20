namespace Managers;

public static class ConsoleManager
{

    ///
    ///     MENU SELECTION FUNCTIONS
    /// 


    /// <summary>
    /// Handles frontend display
    /// </summary>
    public static void LaunchConsoleApp()
    {

        JsonHandler.UnlockJson(GlobalData.lockPath); // Ensure .lock is removed

        LogManager.frontLog.Information("Started Frontend Console Process");

        var options = new List<string> { "Create Task", "View Tasks", "Edit Task", "Remove Task", "Stop Manager -- Not Implemented", "Restart Manager -- Not Implemented", "Exit" }; // List of user options

        // Frontend display loop
        while (true)
        {
            int choice = ShowMenu(options, "Welcome to Task Scheduler"); // Display home menu and get user menu selection

            // Handle user selection
            switch (choice)
            {

                case 0: // Add Task

                    int status = TaskManager.NewTask(); // Create new task and get creation status
                    
                    // Handle task creation errors
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

    /// <summary>
    /// The main 'home' menu to select different views
    /// </summary>
    /// <param name="options">List of views to choose from</param>
    /// <param name="prompt">Intro prompt, eg. 'Welcome to ...'</param>
    /// <returns>Int index of which option you choose; options[selected]</returns>
    private static int ShowMenu(List<string> options, string prompt = "Choose an option:")
    {

        // Creating Variables
        int selected = 0;
        ConsoleKey key;

        // Update loop
        do
        {
            Thread.Sleep(10); // Minimize CPU usage
            
            // Reset display
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
            if (key == ConsoleKey.UpArrow) selected = (selected - 1 + options.Count) % options.Count; // Move selection up
            else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % options.Count; // Move selection down

        } while (key != ConsoleKey.Enter); // Loop while enter hasn't been pressed; nothing has been selected

        return selected; // Return options index
    }

    /// <summary>
    /// Displays all attributes for each saved task
    /// </summary>
    private static void ViewTasks()
    {

        // Check Tasklist
        if (GlobalData.TaskList == null)
        {   // TaskList is null

            ErrorScreen("Could Not View TaskList; TaskList is Null");
            return;
        }

        // Clear and print title
        Console.Clear();
        Console.WriteLine("All Tasks\n(Press any key to go back)\n");

        // Check if the TaskList is empty
        if (GlobalData.TaskList.Count == 0)
        {   // Display no task message

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
        foreach (var item in GlobalData.TaskList)Console.WriteLine($"  {TaskManager.FixedLength(item.TaskName, 8)} | {TaskManager.FixedLength(item.Date, 11)} | {TaskManager.FixedLength(item.Repeats.ToString(), 7)} | {TaskManager.FixedLength(item.RepeatInterval ?? "Null", 6)} | {item.Command}");

        Console.ReadKey(); // Wait for any key press
    }

    /// <summary>
    /// Displays a menu to edit an attribute of a task
    /// </summary>
    private static void EditMenu()
    {
        // Check TaskList
        if (GlobalData.TaskList == null)
        {   // TaskList is null

            ErrorScreen("Could Not Edit TaskList; TaskList is Null");
            return;
        }

        // Create selection variables
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

        // Selection loop
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

            int printLine = 0; // Current row being displayed

            if (GlobalData.TaskList != null)
            {

                // Loop through all tasks in TaskList
                foreach (var item in GlobalData.TaskList)
                {
                    
                    List<string> itemList = TaskManager.TaskToList(item, true); // Get all task attributes in a string array

                    int column = 0; // Current column being displayed
                    
                    Console.Write(" ");
                    foreach (string str in itemList) // Loop through all task attributes
                    {

                        if (printLine == selection[0] && column == selection[1]) // if displaying current selected attribute
                        {   // highlight selected attribute 

                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }

                        Console.Write($" {str} ");
                        Console.ResetColor();

                        if (column != 4) Console.Write("|");

                        column++; // Display next column
                    }
                    Console.Write("\n");

                    printLine++; // Display next row
                }
            }
            else
            {   // Error in displaying edit menu

                ErrorScreen("Could Not Edit TaskList; TaskList is Null");
                return;
            }

            key = Console.ReadKey(true).Key; // Get user input

            // Move user selection
            if (key == ConsoleKey.RightArrow && selection[1] != 4) selection[1]++;
            else if (key == ConsoleKey.LeftArrow && selection[1] != 0) selection[1]--;
            else if (key == ConsoleKey.UpArrow && selection[0] != 0) selection[0]--;
            else if (key == ConsoleKey.DownArrow && selection[0] != GlobalData.TaskList.Count - 1) selection[0]++;

            else if (key == ConsoleKey.Escape) break; // Go back
            else if (key == ConsoleKey.Enter) // Select attribute
            {
                int editStatus = EditAttributeMenu(selection[0], selection[1]); // Edit attribute and get edit status

                // Display edit errors
                if (editStatus == 1) ErrorScreen("Could Not Edit Tasks; TaskList is Null");
                else if (editStatus == 2) ErrorScreen($"Failed to Edit Attribute From Task '{GlobalData.TaskList[selection[0]].TaskName}'");
                else if (editStatus == 3) ErrorScreen($"Failed to Save Changes for Task '{GlobalData.TaskList[selection[0]].TaskName}'");
            }
        }
    }

    /// <summary>
    /// Menu where the user inputs new data for their selected task attribute
    /// </summary>
    /// <param name="taskListIndex">Index of the task being editing in TaskList</param>
    /// <param name="itemIndex">Index of the attribute being edited: [TaskName=0, Date, TaskRepeat?, Command]</param>
    /// <returns>Int status of edit. 0 = Sucessful, 1 = TaskList is null, 2 = Failed to edit attribute, 3 = Failed to save changes</returns>
    private static int EditAttributeMenu(int taskListIndex, int itemIndex)
    {
        List<string> attrList = ["Task Name", "Date", "Task Repeat?", "Repeat Interval", "Command"]; // Create list of all task attributes

        // Check TaskList
        if (GlobalData.TaskList != null)
        {   // Display edit menu

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
        {   // TaskList is null

            ErrorScreen("Could Not Edit Task; TaskList is Null");
            return 1;
        }

        Console.WriteLine("Type '!EXIT?' to go back");

        // Get user input and try editing selected attribute
        if (!TaskManager.EditAttribute(taskListIndex, itemIndex))
        {   // Edit failed

            LogManager.frontLog.Error($"Failed to Edit Attribute '{attrList[itemIndex]}' From Task '{GlobalData.TaskList[taskListIndex].TaskName}'");
            return 2;
        }

        // Try to save changes
        if (!JsonHandler.SaveJsonData()) // Save Changes
        {   // Failed to save changes

            LogManager.frontLog.Error($"Failed to Save Changes for Attribute '{attrList[itemIndex]}' From Task '{GlobalData.TaskList[taskListIndex].TaskName}'");
            return 3;
        }

        LogManager.frontLog.Information($"Edited Attribute '{attrList[itemIndex]}' from Task {GlobalData.TaskList[taskListIndex].TaskName}");
        return 0; // Edit successful
    }
    
    /// <summary>
    /// Displays a menu to select a task to remove
    /// </summary>
    private static void RemoveMenu()
    {
        // Initialize variables
        List<string>? taskNames = TaskManager.GetTaskNames(); // Get task names
        ConsoleKey key; // Console key object
        int selected = 0; // Selected task index

        // Check taskNames
        if (taskNames == null)
        {   // Could not get any tasks

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

                    // Get removal status
                    int removeStatus = TaskManager.RemoveItem(taskNames[selected]);

                    // Check remove status
                    if (removeStatus == 1) ErrorScreen($"Cannot Remove Task '{taskNames[selected]}'; TaskList is Null");
                    else if (removeStatus == 2) ErrorScreen($"Cannot Remove Task '{taskNames[selected]}'; Task Cannot be Found");
                    else
                    {   // Removal successful

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
    private static bool RemoveConfirmation(string name)
    {

        // Reset and write to the console
        Console.Clear();
        Console.WriteLine($"Are you sure you want to delete task: {name}? (YES/NO)");

        string? inp = Console.ReadLine(); // Get user input

        // Make sure the input is valid; either YES or NO
        while (inp == null || (inp.ToUpper() != "YES" && inp.ToUpper() != "NO"))
        {   // Invalid input

            Console.WriteLine("Invalid Input. Try Again (YES/NO)");
            inp = Console.ReadLine();
        }

        // Return true if YES, return false if NO
        if (inp == "YES") return true;
        return false;
    }

    /// <summary>
    /// Displays and logs an error
    /// </summary>
    /// <param name="message">The error message to be displayed</param>
    private static void ErrorScreen(string message)
    {
        LogManager.frontLog.Error($"{message}"); // Log error

        // Display error
        Console.Clear();
        Console.WriteLine($"Error:\n    {message}\n\nPress Enter To Exit");
        Console.ReadLine();
    }
}