# main ToDo
- [X] create home menu screen -- abstract to a GUI eventually
- [X] Finish front end functionality for creating/editing/removing/viewing tasks
- [X] Create separate frontend/backend exe 
- [ ] Create backend functionality
- [ ] Look into combining the frontend and backend into a single exe that can swap between non-headless and headless
- [ ] autostart backend as a Windows Service.
- [ ] Create a log file in AppData Roaming that the backend adds to when a task in run.

## Frontend ToDo
- [X] json file lock when reading/writing
- [X] add View, Edit, and Remove functionality
- [X] Create Home Menu loop that exits with Exit
- [ ] Add option to stop/restart the backend
- [ ] Add reload option to get a updated task list
- [ ] Export tasks list as csv
- [ ] Fix error for checking if a time inputed is valid
- [ ] Check that files are being store to "Program Data"

## GUI
- [ ] Make GUI
- [ ] Add exit functionality to close backend; keep the gui open until the run lock file is gone to ensured the process ended.

## Backend ToDo
- [X] Main loop to check if a task needs to be added to the queue -- loop once a minute
- [X] json file lock when reading
- [X] Create a queue for tasks that need to be run.
- ~~[ ] Make backend into a windows service~~
- [ ] Log file
- [X] add requirement for admin access to run -- might interfer with autostart
- [X] Handle tasks that repeat at the end of a month when the specified day doesn't exist for the current month
- [X] When the backend is running, add it to the system tray. Prompt the frontend when the it is clicked on in the system tray.
- [ ] Add a catch system if the task command fails, add a way to notify the user and add to the log
- [X] Stop a task from being added to the queue if it is already in the queue
- [ ] Change the running program file lock to creating a file to signal termination -- catches all exit possibilities better
- [ ] Launch frontend gui as an entirely new process using -g
- [ ] Test everything