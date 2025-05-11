    using System;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Diagnostics;
    using System.Reflection;
    using System.Collections.Concurrent;
    using System.Timers;

    using Managers;

    class Program
    {
        static ConcurrentQueue<string[]> commandQueue = new ConcurrentQueue<string[]>();

        // Import Windows API to hide/show console window
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;  // Hides the console
        const int SW_SHOW = 5;  // Shows the console
        private static System.Timers.Timer timer;
        

        [STAThread] // Required for WinForms applications
        static void Main(string[] args)
        {

            if (!CheckAdmin()) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Error: Incorrect Permissions\n  Relaunch application with admin priviledges");
                Console.ResetColor();
                Environment.Exit(1);
            }

            // If we're running with the GUI flag, hide the console
            if (args.Contains("-g") || args.Contains("--gui"))
            {
                //var handle = GetConsoleWindow();
                //ShowWindow(handle, SW_HIDE);  // Hide the console

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm()); // Launch your form
            }
            else if (args.Contains("-c") || args.Contains("--console"))
            {
                ConsoleManager.LaunchConsoleApp();
            }
            else if (args.Contains("-s") || args.Contains("--stop")) {
                return; // Add functionality to stop the already running background process
            }
            else
            {
                if (IsGuiAvailable()) {
                    Thread trayThread = new Thread(() => {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        var tray = new Tray();
                        tray.AddToSystemTray();
                        Application.Run();
                    });
                    trayThread.SetApartmentState(ApartmentState.STA);
                    trayThread.IsBackground = true;
                    trayThread.Start();
                }
                // Default backend behavior
                //Console.WriteLine("Backend");
                //while (true) Thread.Sleep(1000);  // Keep console open for debugging

                Thread QueueThread = new Thread(new ThreadStart(QueueManager));
                QueueThread.IsBackground = true;
                QueueThread.Start();

                timer = new System.Timers.Timer(60000);
                timer.Elapsed += TaskLoop;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
        }

        static void TaskLoop(object sender, ElapsedEventArgs e) {
            string currTime = DateTime.Now.ToString("MM/dd HH:mm");
            GlobalData.TaskList = JsonHandler.GetJsonData();
            foreach (var item in GlobalData.TaskList) {
                if (item.Date == currTime && !IsQueued(item.TaskName)) commandQueue.Enqueue(new string[] {item.TaskName, item.Command});
            }
        }

        static bool IsQueued(string taskName) {
            foreach (var item in commandQueue) {
                if (item[0] == taskName) return true;
            }
            return false;
        }

        static public void QueueManager() {
            while (true) {
                if (!commandQueue.IsEmpty && commandQueue.TryDequeue(out string[] front)) {
                    // LOGIC FOR RUNNING FRONT
                    if (front != null) RunPowershell(front[1]);
                    UpdateRepeatTime(front[0]);
                }
                Thread.Sleep(1000);
            }
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

            using (Process process = Process.Start(psi)) {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output)) {
                    // WRITE OUTPUT TO LOG
                    Console.WriteLine($"Powershell output: {output}");
                }

                if (!string.IsNullOrWhiteSpace(error)) {
                    // WRITE ERROR TO LOG
                }
            }
        }

        static string UpdateDate(string date, string interval, int? trueDate = null) {
            switch (interval) {
                case "min": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMinutes(1).ToString("MM/dd HH:mm");
                case "hr": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddHours(1).ToString("MM/dd HH:mm");
                case "mon":
                    if (trueDate != null) {
                        DateTime nextMonth = DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMonths(1);
                        int lastDay = DateTime.DaysInMonth(2024, nextMonth.Month);
                        int day = Math.Min(trueDate.Value, lastDay);
                        return new DateTime(2024, nextMonth.Month, day, nextMonth.Hour, nextMonth.Minute, 0).ToString("MM/dd HH:mm");
                    }
                    else return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddMonths(1).ToString("MM/dd HH:mm");
                case "week": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddDays(7).ToString("MM/dd HH:mm");
                case "day": return DateTime.ParseExact(date, "MM/dd HH:mm", null).AddDays(1).ToString("MM/dd HH:mm");
                case "year": return date;
            }
            return date;
        }

        static void UpdateRepeatTime(string taskName) {
            foreach (var item in GlobalData.TaskList) {
                if (item.TaskName == taskName) {
                    if (item.Repeats) {
                        item.Date = UpdateDate(item.Date, item.RepeatInterval, item.TrueDate);
                        break;
                    }
                    else {
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
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iconPath = Path.Combine(exeDir, "files", "TaskIcon.ico");

            trayIcon = new NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon(iconPath);
            trayIcon.Text = "Sherman Task Scheduler";
            trayIcon.Visible = true;

            trayIcon.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left) {
                    OpenGui();
                }
            };
        }

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
