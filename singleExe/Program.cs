using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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
        // If we're running with the GUI flag, hide the console
        if (args.Contains("-g") || args.Contains("--gui"))
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);  // Hide the console

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // Launch your form
        }
        else if (args.Contains("-c") || args.Contains("--console"))
        {
            ConsoleManager.LaunchConsoleApp();
        }
        else
        {
            // Default backend behavior
            Console.WriteLine("Backend");
            Console.ReadLine();  // Keep console open for debugging
        }
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
