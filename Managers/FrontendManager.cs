namespace Managers;
using System.Diagnostics;

public static class FrontendManager
{
    
}

public class Tray
{// : Form {
    private static NotifyIcon? trayIcon;
    private ContextMenuStrip? trayMenu;
    public void AddToSystemTray()
    {
        string? exeDir = Path.GetDirectoryName(AppContext.BaseDirectory);
        if (exeDir == null)
        {
            LogManager.log.Error("Could Not Get Path to The Working EXE to Create trayIcon; Terminating Backend Process");
            BackendManager.SignalStopBackend();
            return;
        }
        string iconPath = Path.Combine(exeDir, "files", "TaskIcon.ico");

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Open Console View", null, (s, e) => StartGuiProcess("-c"));
        trayMenu.Items.Add("Open GUI View", null, (s, e) => StartGuiProcess("-g"));
        trayMenu.Items.Add("Stop Manager", null, (s, e) => BackendManager.SignalStopBackend());

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
        //LogManager.log.Information("Launching Frontend Process");
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