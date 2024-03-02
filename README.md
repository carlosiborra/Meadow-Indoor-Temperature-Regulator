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
