# MoveMonitorProgram
This is a C# program that allows users to move a selected program window to a selected monitor. The program uses various Windows API functions such as GetWindowRect, MoveWindow, EnumDisplayMonitors, and GetMonitorInfo to interact with the operating system and retrieve information about windows and monitors.

The Form1 class represents the main form of the program, and it contains various methods and event handlers. The LoadRunningProcesses method retrieves a list of all running processes on the system and populates a combobox with their names. The LoadMonitors method retrieves a list of all connected monitors and populates another combobox with their names.

When the user clicks on the "Move" button, the button2_Click event handler is called. This handler retrieves the selected process and selected monitor from the two comboboxes and passes them to the MoveWindowToMonitor method. This method uses the GetWindowRect function to retrieve the position and size of the selected window, calculates the new position of the window on the selected monitor, and uses the MoveWindow function to move the window to the new position.

The program also defines a RECT structure to represent a rectangular area, a MyRect structure to replace the RECT structure in some places, and a MONITORINFOEX structure to represent monitor information. The NativeMethods class is defined to allow access to the MoveWindow function.
