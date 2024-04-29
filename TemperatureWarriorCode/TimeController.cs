using System;
using System.Diagnostics;

public class TimeController
{

    private TemperatureRange[] temperatureRanges;
    private int totalTime;
    private int refreshRate;
    private simplePIDController pidController;

    public bool Configure(TemperatureRange[] ranges, int total, int refresh, out string errorMessage)
    {
        errorMessage = null;

        if (ranges == null || ranges.Length == 0)
        {
            errorMessage = "Temperature ranges cannot be null or empty.";
            return false;
        }

        if (total <= 0)
        {
            errorMessage = "Total time must be greater than zero.";
            return false;
        }

        if (refresh <= 0)
        {
            errorMessage = "Refresh rate must be greater than zero.";
            return false;
        }

        temperatureRanges = ranges;
        totalTime = total;
        refreshRate = refresh;

        return true;
    }

    public void StartOperation()
    {
        // Create a PID controller with the specified gains (kp, ki, kd). TODO: The gains should be tuned based on the system requirements.
        pidController = new PIDController(0.8, 0.2, 0.001);

        pidController.Reset(); // Reset the PID controller

        Stopwatch stopwatch = new Stopwatch(); // Create a stopwatch to measure the time elapsed

        // For each temperature range, run the PID controller until the specified duration is reached
        for (int i = 0; i < temperatureRanges.Length; i++)
        {
            double targetTemperature = (temperatureRanges[i].Min + temperatureRanges[i].Max) / 2.0; // Calculate the target temperature as the average of the minimum and maximum temperature of the range
            stopwatch.Start(); // Start the stopwatch

            while (stopwatch.Elapsed.TotalSeconds < temperatureRanges[i].Duration) // While the elapsed time is less than the duration of the specific range (in seconds)
            {
                double currentTemperature = Data.temp_act; // Obtaining the current temperature.
                double output = pidController.Compute(currentTemperature, targetTemperature); // Compute the PID controller output based on the current temperature and the target temperature
                ApplyControllerOutput(output); // Applying the PID controller output to the system. TODO: ADAPT THIS FUNCTION TO THE ONE USED IN THE PROJECT
            }
        }
        // Detener el cronómetro
        stopwatch.Stop();
    }

    private void ApplyControllerOutput(double output)
    {
        // Apply the PID controller output to the system. This function should be implemented based on the system requirements.
        // The output must be used to control a heater or cooler to maintain the desired temperature. In our case, we have to adapt this output to control the activation and deactivation of the rele.
        // The output value could be used to adjust the power supplied to the heater or cooler.
        // This is a placeholder function and should be replaced with the actual implementation.
        Console.WriteLine("Applying PID controller output: " + output);
    }
}