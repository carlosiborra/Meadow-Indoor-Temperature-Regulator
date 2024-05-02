using System;
using System.Diagnostics;
using Meadow.Foundation.Relays;


public class RoundController
{
    private TemperatureRange[] temperatureRanges;
    private int totalTime;
    private int refreshRate;
    private PIDController pidController;

    private Relay relayBombilla;
    private Relay relayPlaca;

    public bool Configure(TemperatureRange[] ranges, int total, int refresh, Relay relayBombilla, Relay relayPlaca, out string errorMessage)
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

        this.relayBombilla = relayBombilla;
        this.relayPlaca = relayPlaca;

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
                // MAYBE THIS FUNCTION SHOULD BE A THREAD. ADAPT THE PARAMETERS ACCORDING TO THE NEEDS OF THE SYSTEM
                ControlarRelay(relayBombilla, relayPlaca, output, 50, 1000); // Applying the PID controller output to the system.
            }
            stopwatch.Stop();
            stopwatch.Reset();
        }
        stopwatch.Stop();
    }

    private void ControlarRelay(Relay relayBombilla, Relay relayPlaca, int intensidad, int intensityBreackpoint, int periodoTiempo)
    {
        // Intensity BreackPoint es una variable que muestra en que rango deja de enfriar y empieza a calentar
        if (intensidad >= 0 && intensidad <= intensityBreackpoint)
        {
            // Código de enfriamiento

            // Calculamos el tiempo de encendido proporcional a la intensidad
            int tiempoEncendido = intensidad * (100/intensityBreackpoint) * periodoTiempo / 100;
            // Encendemos el relay de la placa de Peltier para enfriar y apagamos la bombilla
            relayPlaca.IsOn = true;
            relayBombilla.IsOn = false;
            // Esperar el tiempo de encendido
            Thread.Sleep(tiempoEncendido);

            int tiempoApagado = periodoTiempo - tiempoEncendido;
            // Apagar el relay el tiempo proporcional de apagado
            relayPlaca.IsOn = false;
            Thread.Sleep(tiempoApagado);
        }
        else if (intensidad >= intensityBreackpoint && intensidad <= 100)
        {
            // Código de calentamiento

            // Calculamos el tiempo de encendido proporcional a la intensidad
            int tiempoEncendido = intensidad * (1-(100 / intensityBreackpoint)) * periodoTiempo / 100;
            // Encendemos el relay de la bombilla  y apagamos placa de Peltier
            relayBombilla.IsOn = false;
            relayPlaca.IsOn = true;
            // Esperar el tiempo de encendido
            Thread.Sleep(tiempoEncendido);

            int tiempoApagado = periodoTiempo - tiempoEncendido;
            // Apagar el relay el tiempo proporcional de apagado
            relayPlaca.IsOn = false;
            Thread.Sleep(tiempoApagado);
        }
        else
        {
            Console.WriteLine("Error: La intensidad debe estar en el rango de 0 a 100.");
        }
    }
}
