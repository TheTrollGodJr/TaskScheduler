/*
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        var isService = !(Debugger.IsAttached || args.Contains("--console"));

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<BackendWorker>();
            });

        if (isService)
        {
            builder.UseWindowsService(); // Runs as a service
        }
        else
        {
            builder.UseConsoleLifetime(); // Runs as console app
        }

        await builder.Build().RunAsync();
    }
}
*/




using Managers;

class TaskService
{
    //[STAThread]
    static void Main(string[] args) {

        // Check for correct permissions
        if (!BackendManager.CheckAdmin())
        {   // Launched without admin

            LogManager.log.Error("Launched with incorrect permissions");
            throw new Exception("Incorrect Permissions, Relaunch Application With Admin Priviledges");
        }
        
        Directory.CreateDirectory(BackendManager.appDir); // Create data folder in Program Data if not already made

        if (BackendManager.CheckFlags(args)) return; // Check exe flags, run backend if none are present
        BackendManager.IsBackendAlreadyRunning(); // Throws an error if an instance of the backend process is already running

        // Flag that toggles the backend console being hidden; usually for debugging
        if (!args.Contains("--show-ui"))
        {   // Hide the console

            var handle = BackendManager.GetConsoleWindow();
            BackendManager.ShowWindow(handle, BackendManager.SW_HIDE);
        }

        // Start logs
        LogManager.log.Information("Starting Backend");
        LogManager.CleanOldLogs();

        /*
        // Create tray icon
        if (BackendManager.IsGuiAvailable()) // Ensure a gui is avaliable
        {

            // Create tray icon thread
            Thread trayThread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var tray = new Tray();
                tray.AddToSystemTray();
                Application.Run();
            });

            // Start thread
            trayThread.SetApartmentState(ApartmentState.STA);
            trayThread.IsBackground = true;
            trayThread.Start();
            LogManager.log.Information("Started TrayIcon Thread");
        }
        */

        // Create a timer thread
        BackendManager.TimerThread = new Thread(new ThreadStart(BackendManager.TaskLoop));
        BackendManager.TimerThread.IsBackground = true;
        BackendManager.TimerThread.Start();
        LogManager.log.Information("Started Timer Thread");

        // Run backend
        bool queueStatus = BackendManager.QueueLoop(); // Manages the queue, returns false if there is an error
        if (!queueStatus) LogManager.log.Error("Queue Loop Failed"); // Log error

        BackendManager.CloseBackend(); // Shutdown the backend
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
