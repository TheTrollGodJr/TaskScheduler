using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using System.Timers;
using System.ServiceProcess;
using System.IO;
using Serilog;

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

    [STAThread]
    static void Main(string[] args) {

        if (!CheckAdmin())
        {
            GlobalData.log.Error("Launched with incorrect permissions");
            throw new Exception("Incorrect Permissions, Relaunch Application With Admin Priviledges");
            //            Console.WriteLine("Error: Incorrect Permissions\n  Relaunch application with admin priviledges");
            //            Environment.Exit(1);
        }

        
        Directory.CreateDirectory(appDir);

        // If we're running with the GUI flag, hide the console
        if (args.Contains("-g") || args.Contains("--gui"))
        {
            GlobalData.log.Information("Starting GUI Process");
            if (!args.Contains("--show-ui"))
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);  // Hide the console
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // Launch your form
            GlobalData.log.Information("Ending GUI Process");
            return;
        }
        else if (args.Contains("-c") || args.Contains("--console"))
        {
            GlobalData.log.Information("Starting Console Frontend Process");
            ConsoleManager.LaunchConsoleApp();
            GlobalData.log.Information("Ending Console Frontend Process");
            return;
        }
        else if (args.Contains("-s") || args.Contains("--stop")) {
            if (File.Exists(runLock)) File.Delete(runLock);
            GlobalData.log.Information("Removed RunLock File to Stop The Backend Process");
            return; // Add functionality to stop the already running background process
        }

        ///
        /// HIDE CONSOLE WINDOW
        /// 
        if (!args.Contains("--show-ui")) {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        if (!File.Exists(runLock)) File.Create(runLock).Close();
        else {
                ///////////////////// ---- TEST THIS
            GlobalData.log.Error("Failed to Start Backend Process: Backend is Already Running");
            throw new Exception("Error: Background Process Already Running\nTo force stop use the flag '--stop'");
        }

        GlobalData.log.Information("Starting Backend");

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
            GlobalData.log.Information("Started TrayIcon Thread");
        }

        Thread TimerThread = new Thread(new ThreadStart(TaskLoop));
        TimerThread.IsBackground = true;
        TimerThread.Start();
        GlobalData.log.Information("Started Timer Thread");

        while (true)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            if (!commandQueue.IsEmpty && commandQueue.TryDequeue(out string[] front))
            {
                // LOGIC FOR RUNNING FRONT
                //Console.WriteLine($"Running task: {front[0]}");
                GlobalData.log.Information($"Running Task: {front[0]}");
                if (front != null) RunPowershell(front[1]);
                UpdateRepeatTime(front[0]);
            }
            if (!File.Exists(runLock))
            {
                GlobalData.log.Information("Ending Backend Process");
                runTimer = false;
                TimerThread.Join();
                GlobalData.log.Information("Backend Process Terminated");
                break;
            }
        }

        File.Delete(runLock);
    }


    static void TaskLoop() {
        while (runTimer) {
            string currTime = DateTime.Now.ToString("MM/dd HH:mm");
            //Console.WriteLine($"Task Looped: {currTime}");
            GlobalData.TaskList = JsonHandler.GetJsonData();
            foreach (var item in GlobalData.TaskList)
            {
                //Console.WriteLine($"    {item.TaskName}, {item.Date}, {item.RepeatInterval}");
                if (item.Date == currTime && !IsQueued(item.TaskName))
                {
                    GlobalData.log.Information($"Task '{item.TaskName}' Added to The Queue");
                    commandQueue.Enqueue(new string[] { item.TaskName, item.Command });
                }
            }
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }

    static bool IsQueued(string taskName)
    {
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

        GlobalData.log.Information($"Running PowerShell Command: {command}");

        using (Process process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                // WRITE OUTPUT TO LOG
                GlobalData.log.Information($"PowerShell Output: {output}");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                // WRITE ERROR TO LOG
                GlobalData.log.Error($"PowerShell Error: {error}");
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
        
        foreach (var item in GlobalData.TaskList)
        {
            if (item.TaskName == taskName)
            {
                if (item.Repeats)
                {
                    GlobalData.log.Information($"Updating Repeat Interval For Task '{taskName}'");
                    item.Date = UpdateDate(item.Date, item.RepeatInterval, item.TrueDate);
                    JsonHandler.SaveJsonData();
                    break;
                }
                else
                {
                    GlobalData.log.Information($"Removing Task '{taskName}'");
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

public class Tray {// : Form {
    private NotifyIcon trayIcon;
    private Thread guiThread;
    private MainForm guiForm;
    public void AddToSystemTray() {
        string exeDir = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        string iconPath = Path.Combine(exeDir, "files", "TaskIcon.ico");

        trayIcon = new NotifyIcon();
        trayIcon.Icon = new System.Drawing.Icon(iconPath);
        trayIcon.Text = "Sherman Task Scheduler";
        trayIcon.Visible = true;

        trayIcon.MouseClick += (sender, e) => {
            if (e.Button == MouseButtons.Left) {
                StartGuiProcess();
            }
        };
    }

/*
    void OpenGui() {
        if (guiForm != null && !guiForm.IsDisposed) {
            guiForm.Invoke(new Action(() => {
                if (!guiForm.Visible) guiForm.Show();
                guiForm.BringToFront();
            }));
            return;
        }

        guiThread = new Thread(() => {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            guiForm = new MainForm();
            Application.Run(guiForm);
        });

        guiThread.SetApartmentState(ApartmentState.STA); // WinForms requires STA
        guiThread.IsBackground = true;
        guiThread.Start();
    }
    */

    void StartGuiProcess() {
        GlobalData.log.Information("Launching Frontend Process");
        string exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
        string exePath = Path.Combine(AppContext.BaseDirectory, exeName);

        Process.Start(new ProcessStartInfo {
            FileName = exePath,
            Arguments = "-g",
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
