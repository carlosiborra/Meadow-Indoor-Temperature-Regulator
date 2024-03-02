# Sistemas-software-Grupo-1

## Steps to Install Meadow (Win11)

Follow these steps to install Meadow on Windows 11:

1. **Install .NET 8.0 SDK**

2. **Install VS Code Community Edition**
   - Install the Meadow extension
   - Disable automatic updates

3. **In the Terminal**
   - Install Meadow CLI tool:
     ```bash
     dotnet tool install WildernessLabs.Meadow.CLI --global
     ```
   - List available ports to find the Meadow port:
     ```bash
     meadow port list
     ```
   - Configure Meadow to use a specific port (e.g., COM5):
     ```bash
     meadow config route COM5
     ```
   - Retrieve device information:
     ```bash
     meadow device info
     ```

## Steps to Run a Meadow Project

Follow these steps to run a Meadow project:
    1. Select the Meadow project in VS Code
    2. Press F5 to build and deploy the project to the Meadow board

Note: beware, you should douvle click TemperatureWarriorCode.sln to open the project in VS Code, otherwise it will not work.