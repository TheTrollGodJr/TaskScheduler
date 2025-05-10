    using System;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Diagnostics;
    using System.Reflection;

    using Managers;

    class Program
    {
        
        // Import Windows API to hide/show console window
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;  // Hides the console
        const int SW_SHOW = 5;  // Shows the console
        

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
                while (true) Thread.Sleep(1000);  // Keep console open for debugging
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
