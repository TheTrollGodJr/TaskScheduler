using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Collections.Concurrent;

using Managers;

class TaskService
{
    static ConcurrentQueue<string[]> commandQueue = new ConcurrentQueue<string[]>();

    // Import Windows API to hide/show console window
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;  // Hides the console
    const int SW_SHOW = 5;  // Shows the console
    private static bool runTimer = true;
    private static string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ShermanTaskScheduler");
    private static string runLock = Path.Combine(appDir, "running.lock");
    private static string stopSignal = Path.Combine(appDir, "stopSignal.lock");
    private static Thread? TimerThread;
    private static readonly ManualResetEvent shutdownEvent = new(false);

    [STAThread]
    static void Main(string[] args) {

        if (!CheckAdmin())
        {
            LogManager.log.Error("Launched with incorrect permissions");
            throw new Exception("Incorrect Permissions, Relaunch Application With Admin Priviledges");
            //            Console.WriteLine("Error: Incorrect Permissions\n  Relaunch application with admin priviledges");
            //            Environment.Exit(1);
        }
        
        Directory.CreateDirectory(appDir);

        if (CheckFlags(args)) return;
        IsBackendRunning();

        if (!args.Contains("--show-ui"))
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        LogManager.log.Information("Starting Backend");
        LogManager.CleanOldLogs();

        if (IsGuiAvailable())
        {
            Thread trayThread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var tray = new Tray();
                tray.AddToSystemTray();
                Application.Run();
            });
            trayThread.SetApartmentState(ApartmentState.STA);
            trayThread.IsBackground = true;
            trayThread.Start();
            LogManager.log.Information("Started TrayIcon Thread");
        }

        TimerThread = new Thread(new ThreadStart(TaskLoop));
        TimerThread.IsBackground = true;
        TimerThread.Start();
        LogManager.log.Information("Started Timer Thread");

        bool queueStatus = QueueLoop();
        if (!queueStatus) LogManager.log.Error("Queue Loop Failed");

        CloseBackend();
    }

    static bool QueueLoop()
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
                    UpdateRepeatTime(front[0]);
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

    static void IsBackendRunning()
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

    static bool CheckFlags(string[] args)
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

    public static void SignalStopBackend()
    {
        if (!File.Exists(stopSignal))
        {
            File.Create(stopSignal).Close();
            shutdownEvent.Set();
            LogManager.log.Information("Created Stop Signal File");
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

    static void TaskLoop()
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

    static bool IsQueued(string? taskName)
    {
        if (taskName == null) return false;

        foreach (var item in commandQueue)
        {
            if (item[0] == taskName) return true;
        }
        return false;
    }


    static void RunPowershell(string command) {
        ProcessStartInfo psi = new ProcessStartInfo {
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

    static void UpdateRepeatTime(string taskName) {
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
                    TaskManager.RemoveItem(taskName);
                    break;
                }
            }
        }
    }

    static bool CheckAdmin() {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    static bool IsGuiAvailable() {
        try {
            return Environment.UserInteractive
                && !string.IsNullOrEmpty(SystemInformation.UserName);
        }
        catch {
            return false;
        }
    }
}

public class Tray
{// : Form {
    private static NotifyIcon? trayIcon;
    private ContextMenuStrip? trayMenu;
    public void AddToSystemTray()
    {
        string? exeDir = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        if (exeDir == null)
        {
            LogManager.log.Error("Could Not Get Path to The Working EXE to Create trayIcon; Terminating Backend Process");
            TaskService.SignalStopBackend();
            return;
        }
        string iconPath = Path.Combine(exeDir, "files", "TaskIcon.ico");

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Open Console View", null, (s, e) => StartGuiProcess("-c"));
        trayMenu.Items.Add("Open GUI View", null, (s, e) => StartGuiProcess("-g"));
        trayMenu.Items.Add("Stop Manager", null, (s, e) => TaskService.SignalStopBackend());

        trayIcon = new NotifyIcon();
        trayIcon.Icon = new System.Drawing.Icon(iconPath);
        trayIcon.Text = "Sherman Task Scheduler";
        trayIcon.Visible = true;
        trayIcon.ContextMenuStrip = trayMenu;

        trayIcon.MouseClick += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                StartGuiProcess("-c");
            }
        };

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    static void OnProcessExit(object? sender, EventArgs e)
    {
        if (trayIcon != null)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
    }

    void StartGuiProcess(string arg)
    {
        LogManager.log.Information("Launching Frontend Process");
        var file = Process.GetCurrentProcess().MainModule?.FileName;//.MainModule.FileName;
        if (file == null)
        {
            LogManager.log.Error("Failed to Launch The Frontend Process; Could Not Get Path to The Working Exe");
            return;
        }

        string exeName = Path.GetFileName(file);
        string exePath = Path.Combine(AppContext.BaseDirectory, exeName);

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arg,
            UseShellExecute = true,
            Verb = "Runas"
        });
    }
}


public class MainForm : Form
{
    public MainForm()
    {
        Text = "My Windows Forms App";
        Width = 300;
        Height = 200;
        Button btn = new Button
        {
            Text = "Click Me",
            Dock = DockStyle.Fill
        };
        btn.Click += (sender, e) => MessageBox.Show("Hello from Windows Forms!");
        Controls.Add(btn);
    }
}
