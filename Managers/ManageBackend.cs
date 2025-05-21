using System.Security.Principal;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Managers;

public static class BackendManager
{
    private static bool runTimer = true;
    public static string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler");
    private static string runLock = Path.Combine(appDir, "running.lock");
    private static string stopSignal = Path.Combine(appDir, "stopSignal.lock");
    public static Thread? TimerThread;
    private static readonly ManualResetEvent shutdownEvent = new(false);
    static ConcurrentQueue<string[]> commandQueue = new ConcurrentQueue<string[]>();

    // Import Windows API to hide/show console window
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int SW_HIDE = 0;  // Hides the console
    public const int SW_SHOW = 5;  // Shows the console


    ///
    ///     PUBLIC FUNCTIONS
    /// 


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool CheckAdmin()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static void TaskLoop()
    {
        while (runTimer)
        {
            string currTime = DateTime.Now.ToString("MM/dd HH:mm");
            //Console.WriteLine($"Task Looped: {currTime}");
            GlobalData.TaskList = JsonHandler.GetJsonData();

            if (GlobalData.TaskList == null)
            {
                LogManager.log.Error("Task Loop Terminated; TaskList is Null");
                runTimer = false;
                break;
            }

            foreach (var item in GlobalData.TaskList)
            {

                if (item.Date == currTime && !IsQueued(item.TaskName) && item.TaskName != null && item.Command != null)
                {
                    LogManager.log.Information($"Task '{item.TaskName}' Added to The Queue");
                    commandQueue.Enqueue([item.TaskName, item.Command]);
                }
            }
            //Thread.Sleep(TimeSpan.FromMinutes(1));
            if (shutdownEvent.WaitOne(TimeSpan.FromMinutes(1)))
            {
                LogManager.log.Information("Task Loop Shutdown Signal Recieved; Closing Task Loop Thread");
                break;
            }
        }
    }

    public static void CloseBackend()
    {
        if (File.Exists(runLock)) File.Delete(runLock);
        LogManager.log.Information("Removed RunLock File");

        if (File.Exists(stopSignal)) File.Delete(stopSignal);
        LogManager.log.Information("Removed Stop Signal File");

        if (File.Exists(GlobalData.lockPath)) File.Delete(GlobalData.lockPath);
        LogManager.log.Information("Removed tasks.json Lock File");
    }

    public static void SignalStopBackend()
    {
        if (!File.Exists(stopSignal))
        {
            File.Create(stopSignal).Close();
            shutdownEvent.Set();
            LogManager.log.Information("Created Stop Signal File");
        }
    }

    public static bool CheckFlags(string[] args)
    {
        switch (args[0])
        {
            case "-c":
            case "--console":
                LogManager.log.Information("Starting Console Frontend Process");
                ConsoleManager.LaunchConsoleApp();
                LogManager.log.Information("Ending Console Frontend Process");
                return true;
            case "-g":
            case "--gui":
                LogManager.log.Information("Starting GUI Process");
                if (!args.Contains("--show-ui"))
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm()); // Launch your form
                LogManager.log.Information("Ending GUI Process");
                return true;
            case "-t":
            case "--tray":
                break;
            /// ------------ CODE TO LAUNCH TRAY ICON AS NEW PROCESS
            case "-s":
            case "--stop":
                LogManager.log.Information("Stop Flag Signaled From External Process");
                SignalStopBackend();
                return true;
            case "-fs":
            case "--force-stop":
                LogManager.log.Information("Force Stop Flag Signaled From External Process");
                CloseBackend();
                return true;
            case "-cl":
            case "--clear-logs":
                LogManager.ClearAllLogs();
                return true;
        }
        return false;
    }

    /*
    public static bool CheckFlags_(string[] args) ///-------- CHANGE TO SWITCH STATEMENT
    {
        // If we're running with the GUI flag, hide the console
        if (args.Contains("-g") || args.Contains("--gui"))
        {
            LogManager.log.Information("Starting GUI Process");
            if (!args.Contains("--show-ui"))
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);  // Hide the console
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // Launch your form
            LogManager.log.Information("Ending GUI Process");
            return true;
        }
        else if (args.Contains("-c") || args.Contains("--console"))
        {
            LogManager.log.Information("Starting Console Frontend Process");
            ConsoleManager.LaunchConsoleApp();
            LogManager.log.Information("Ending Console Frontend Process");
            return true;
        }
        else if (args.Contains("-s") || args.Contains("--stop"))
        {
            LogManager.log.Information("Stop Flag Signaled From External Process");
            SignalStopBackend();
            return true;
        }
        else if (args.Contains("-fs") || args.Contains("--force-stop"))
        {
            LogManager.log.Information("Force Stop Flag Signaled From External Process");
            CloseBackend();
            return true;
        }
        else if (args.Contains("-cl") || args.Contains("--clear-logs"))
        {
            LogManager.ClearAllLogs();
            return true;
        }

        return false;
    }
    */

    public static void IsBackendAlreadyRunning()
    {
        if (File.Exists(stopSignal))
        {
            LogManager.log.Error("Failed to Start Backend Process; Stop Signal Still Exists");
            throw new Exception("Error: Failed to Start Backend Process; Stop Signal Still Exists\nTo Force Stop Use The Flag '--force-stop'");
        }
        else if (!File.Exists(runLock)) File.Create(runLock).Close();
        else
        {
            LogManager.log.Error("Failed to Start Backend Process: Backend is Already Running");
            throw new Exception("Error: Background Process Already Running\nTo Force Stop Use The Flag '--force-stop'");
        }
    }

    public static bool QueueLoop()
    {
        if (TimerThread == null) return false;

        while (true)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            if (!commandQueue.IsEmpty && commandQueue.TryDequeue(out string[]? front))
            {
                // LOGIC FOR RUNNING FRONT
                //Console.WriteLine($"Running task: {front[0]}");

                if (front != null)
                {
                    LogManager.log.Information($"Running Task: {front[0]}");
                    RunPowershell(front[1]);
                    TaskManager.UpdateRepeatTime(front[0]);
                }
            }
            if (File.Exists(stopSignal))
            {
                LogManager.log.Information("Ending Backend Process");
                runTimer = false;
                TimerThread.Join();
                LogManager.log.Information("Backend Process Terminated");
                break;
            }
            else if (!File.Exists(runLock))
            {
                LogManager.log.Error("Run Lock Unexpected Removed; Terminating Backend");
                runTimer = false;
                TimerThread.Join();
                LogManager.log.Information("Backend Process Terminated");
            }
            if (!runTimer)
            {
                LogManager.log.Error("runTimer Stopped Prematurely; Terminating Backend process");
                break;
            }
        }

        return true;
    }

    public static bool IsGuiAvailable() {
        try {
            return Environment.UserInteractive
                && !string.IsNullOrEmpty(SystemInformation.UserName);
        }
        catch {
            return false;
        }
    }

    ///
    ///     PRIVATE FUNCTIONS
    /// 


    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    private static void RunPowershell(string command)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        LogManager.log.Information($"Running PowerShell Command: {command}");

        using (Process? process = Process.Start(psi))
        {
            if (process == null)
            {
                LogManager.log.Error("Could Not Run PowerShell Command; PowerShell Process Was Null");
                return;
            }
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                // WRITE OUTPUT TO LOG
                LogManager.log.Information($"PowerShell Output:\n{output}");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                // WRITE ERROR TO LOG
                LogManager.log.Error($"PowerShell Error:\n{error}");
            }
        }
    }

    
    private static bool IsQueued(string? taskName)
    {
        if (taskName == null) return false;

        foreach (var item in commandQueue)
        {
            if (item[0] == taskName) return true;
        }
        return false;
    }

    

    

    

    

    
}