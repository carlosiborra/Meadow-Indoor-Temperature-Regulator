# Software Systems Group 1

Welcome to the Software Systems Group 1's GitHub repository. This README provides detailed instructions for installing and running Meadow on Windows 11, specifically tailored for our project needs.

## 🚀 Installation Guide for Meadow on Windows 11

To get Meadow set up and running on your Windows 11 system, please follow the steps outlined below:

### Prerequisites

- **.NET 8.0 SDK**: Ensure you have the .NET 8.0 SDK installed on your machine.

### 1. Install Visual Studio Code Community Edition

- Download and install [VS Code Community Edition](https://code.visualstudio.com/).
- Within VS Code, proceed to install the Meadow extension to aid in Meadow development.
- It's advisable to disable automatic updates in VS Code to maintain a consistent development environment.

### 2. Set Up Meadow CLI

Open your terminal or command prompt and execute the following commands:

- **Install Meadow CLI Tool**:
  ```bash
  dotnet tool install WildernessLabs.Meadow.CLI --global
  ```
- **Identify Meadow Port**:
  Find out which port your Meadow board is connected to:
  ```bash
  meadow port list
  ```
- **Configure Meadow Port**:
  Configure the CLI to communicate with your Meadow board via the identified port (e.g., COM5):
  ```bash
  meadow config route COM5
  ```
- **Verify Device Information**:
  Retrieve and verify your Meadow device information:
  ```bash
  meadow device info
  ```

## 🏃‍♂️ Running a Meadow Project

To deploy and run a project on your Meadow board, follow these instructions:

1. **Open Project**:
   Double-click on `TemperatureWarriorCode.sln` to open the project in Visual Studio Code.

2. **Build and Deploy**:
   Select the Meadow project within VS Code, and press `F5` to build and deploy the project to your Meadow board.

> **Note**: It's crucial to open the project by double-clicking `TemperatureWarriorCode.sln`. Opening the project in any other manner may result in it not functioning correctly.


## Api endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| **GET** | /temp | Returns the current temperature |
| **GET** | /setparams | Query Params *pass*, *temp_max*, *temp_min*, *display_refresh*, *refresh*, *round_time*  |
| **GET** | /start | Starts the round |
| **POST** | /shutdown | Shutdown the device |