

/// -------- MAKE SURE A TASK ISN'T IN THE QUEUE MORE THAN ONCE -----------
/// 
/// -------- KEEP FRONTEND AND BACKEND TASK LISTS SEPARATE ALWAYS -----------

using System;
using System.Threading;
using Shared;

class Program {

    static void main(string[] args) {
        Queue<ScheduledTask> Q;
        Timer _timer = new Timer(UpdateTaskList, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

    }

    static void UpdateTaskList(object state) {
        GlobalData.TaskList = jsonHandler.GetJsonData();
    }
}
