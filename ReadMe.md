# main ToDo
- [X] create home menu screen -- abstract to a GUI eventually
- [ ] Finish front end functionality for creating/editing/removing/viewing tasks
- [ ] Create separate backend exe that autostarts as a Windows Service.
    - [ ] Create a log file in AppData Roaming that the backend adds to when a task in run.

## Frontend ToDo
- [X] json file lock when reading/writing
- [ ] add View, Edit, and Remove functionality
- [X] Create Home Menu loop that exits with Exit
- [ ] Add option to stop/restart the backend

## Backend ToDo
- [ ] Main loop to check if a task needs to be added to the queue -- loop once a minute
- [ ] json file lock when reading
- [ ] Create a queue for tasks that need to be run -- create a separate thread to run tasks. Create no more than three threads at once, everything else waits in the queue to be run.
- [ ] Figure out how to add to Windows Service to autostart
- [ ] Log file
- [ ] add requirement for admin access to run -- might interfer with autostart
- [ ] Handle tasks that repeat at the end of a month when the specified day doesn't exist for the current month
- [ ] When the backend is running, add it to the system tray. Prompt the frontend when the it is clicked on in the system tray.