

/// -------- MAKE SURE A TASK ISN'T IN THE QUEUE MORE THAN ONCE -----------
/// 
/// -------- KEEP FRONTEND AND BACKEND TASK LISTS SEPARATE ALWAYS -----------

using System;
using System.Threading;
using Shared;
using System.Drawing;
using System.Windows.Forms;

class Program {
    private static bool _isRunning = false;
    static string now = DateTime.Now.ToString("MM/dd HH:mm");
    private Queue<ScheduledTask> Q;
    static List<string> toRemove = [];
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;

    [STAThread]
    static void Main(string[] args) {
        Application
    }

    /*
    static void Main(string[] args) {
        
        Timer _timer = new Timer(EventLoop, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

    }
    */

    static void EventLoop(object state) {
        if (_isRunning) return;
        _isRunning = true;
        GlobalData.TaskList = jsonHandler.GetJsonData();
        /// ---- Create a log of any new or removed tasks ---- ///
        foreach (var item in GlobalData.TaskList) {
            if (item.Date == now) RunCommand(item.Command);
            UpdateDate(item);
        }


        foreach (var name in toRemove) RemoveItem(name);  // Log
        toRemove.Clear();
        
        jsonHandler.SaveJsonData();
        _isRunning = false;
    }

    static void RunCommand(string command) {
        // Create and manage threads to run commands
        // Log
    }

    static void UpdateDate(ScheduledTask item) {
        // Log
        DateTime cur = DateTime.ParseExact(item.Date, "MM/dd HH:mm", null);
        if (item.Repeats) {
            switch (item.RepeatInterval) {
                case "min":
                    item.Date = cur.AddMinutes(1).ToString("MM/dd HH:mm");
                    // Log
                    break;
                case "hr":
                    item.Date = cur.AddHours(1).ToString("MM/dd HH:mm");
                    // log
                    break;
                case "day":
                    item.Date = cur.AddDays(1).ToString("MM/dd HH:mm");
                    // log
                    break;
                case "week":
                    item.Date = cur.AddDays(7).ToString("MM/dd HH:mm");
                    // log
                    break;
                case "mon":
                    item.Date = cur.AddMonths(1).ToString("MM/dd HH:mm");
                    // log
                    break;
                case null: break;
            }
        }
        else toRemove.Add(item.TaskName);
    }

    public static void RemoveItem(string name) {

        int index; // index of the item to be removed

        // Loop through all tasks until the task is found
        for (index = 0; index < GlobalData.TaskList.Count; index++) {
            if (GlobalData.TaskList[index].TaskName == name) break;
        }

        // Do nothing if the task wasn't found
        if (index >= GlobalData.TaskList.Count) return;

        GlobalData.TaskList.RemoveAt(index); // Remove selected task
        jsonHandler.SaveJsonData(); // Save changes
    }
}
