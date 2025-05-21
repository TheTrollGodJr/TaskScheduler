# main ToDo
- [X] create home menu screen -- abstract to a GUI eventually
- [X] Finish front end functionality for creating/editing/removing/viewing tasks
- [X] Create separate frontend/backend exe 
- [X] Create backend functionality
- [X] Look into combining the frontend and backend into a single exe that can swap between non-headless and headless
- [ ] ~~autostart backend as a Windows Service~~
- [X] Create log files
- [ ] Add comments to all frontend
- [ ] Add comments to all backend
- [ ] Reorganize all code
- [ ] Create installer exe using Inno Setup/Look into creating msi installer
- [ ] Add SHA-256 hash to release for extra security
- [ ] Create actual ReadMe.md with project info

## Frontend ToDo
- [X] json file lock when reading/writing
- [X] add View, Edit, and Remove functionality
- [X] Create Home Menu loop that exits with Exit
- [ ] Add option to stop/restart the backend
- [ ] Add refresh option to get a updated task list
- [ ] Export tasks list as csv
- [X] Fix error for checking if a time inputed is valid
- [X] Check that files are being store to "Program Data"
- [X] Handle edge cases when TaskList is null
- [ ] Add a way to exit the NewTask() screen
- [ ] Distinguish between error logs and fatal logs

## GUI
- [ ] Make GUI
- [ ] Add exit functionality to close backend; keep the gui open until the run lock file is gone to ensured the process ended.

## Backend ToDo
- [X] Main loop to check if a task needs to be added to the queue -- loop once a minute
- [X] json file lock when reading
- [X] Create a queue for tasks that need to be run.
- [X] Log file
- [X] add requirement for admin access to run -- might interfer with autostart
- [X] Handle tasks that repeat at the end of a month when the specified day doesn't exist for the current month
- [X] When the backend is running, add it to the system tray. Prompt the frontend when the it is clicked on in the system tray.
- [X] Add a catch system if the task command fails, add a way to notify the user and add to the log
- [X] Stop a task from being added to the queue if it is already in the queue
- [X] Launch frontend gui as an entirely new process using -g
- [X] Test everything
- [X] Move logs to a subfolder
- [X] Remove all logs over 7 days old
- [X] Add UAC prompt
- [X] Create stop signal file instead of removing running.lock -- keep running to lock to prevent lauching more backend processes
- [X] Make backend close quicker by interrupting the TaskLoop Thread.Sleep() instead of waiting a full minute
- [ ] Clean up the main Program.cs; move most functions into different files
- [ ] Distinguish between error logs and fatal logs

# Windows Service
- [ ] Add service functionality by making a subclass from BackgroundService from Microsoft.Extensions.Hosting
- [ ] adapt the queue loop to check for a cancellation token
- [ ] launch tray icon as a process in the user session on login -- do this on every login
- [ ] 

# Tray process:
- [ ] add check for stop signal/canellation token to end tray icon
