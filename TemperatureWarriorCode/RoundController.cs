using System;
using System.Diagnostics;
using Meadow.Foundation.Relays;
using System.Threading;
using TemperatureWarriorCode;
using NETDuinoWar;
using System.Runtime.InteropServices;
using System.Xml.Schema;

public class RoundController
{
    private TemperatureRange[] temperatureRanges;
    private int totalTime;
    private int refreshRate;
    private PIDController pidController;

    private Relay relayBombilla;
    private Relay relayPlaca;

    //Time Controller Values
    public static int total_time_in_range = 0;
    public static int total_time_out_of_range = 0;

    public static readonly double min_allowed_temp = -12.0; // In ºC
    public static readonly double max_allowed_temp = 55.0; // In ºC
    private static readonly double max_temp_comp = 38.0; // In ºC
    private static readonly double min_temp_comp = 12.0; // In ºC

    static int SigmoidInt(int x)
    {
        x = x / 100;
        double result = 1 / (1 + Math.Exp(-x));
        return (int)Math.Round(result * 100);
    }

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

    public double CheckTemperature(double temperature)
    {
        if (temperature > max_allowed_temp)
        {
            throw new ArgumentException($"Max temperature should be less than {max_allowed_temp}ºC.");
        }

        if (temperature < min_temp_comp || temperature > max_temp_comp)
        {
            throw new ArgumentException($"Temperature range should be between {min_temp_comp}ºC and {max_temp_comp}ºC.");
        }
        return temperature;
    }

    public void StartOperation(TimeController timeController, int total_time)
    {
        Console.WriteLine("Starting the operations (PID and relayController)...");

        double kp = 9.0;
        double ki = 0.12;
        double kd = 0.09;

        pidController = new PIDController(kp, ki, kd);
        pidController.Reset(); // Reset the PID controller

        // Inicializamos una variable de condición
        ManualResetEventSlim condicion = new ManualResetEventSlim(false);
        ManualResetEventSlim condicion_PID = new ManualResetEventSlim(false);

        // Use ThreadPool to manage thread creation
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Console.WriteLine("Esperando a recibir una señal...");
            condicion.Wait(); // El hilo espera en la condición
            Console.WriteLine("Señal recibida");

            while (true)
            {
                try
                {
                    // Calcular la salida del control PID
                    condicion_PID.Wait();
                    // Obtener la salida del control PID
                    int output = (int)Data.output;
                    // Use a lock
                    lock (Data.temp_act)
                    {
                        Data.temp_structure.temperatures.Add(Convert.ToDouble(Data.temp_act));
                        timeController.RegisterTemperature(Convert.ToDouble(Data.temp_act));
                        Data.temp_structure.temp_max.Add(Convert.ToDouble(Data.temperaturaMaxima));
                        Data.temp_structure.temp_min.Add(Convert.ToDouble(Data.temperaturaMinima));
                        Data.temp_structure.timestamp.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    }
                    Thread.Sleep(Data.refresh);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in PID computation thread: {ex.Message}");
                }
            }
        });

        ThreadPool.QueueUserWorkItem(_ =>
        {
             while (true)
            {
                try
                {
                    // Calcular la salida del control PID
                    pidController.Compute(Data.targetTemperature);
                    condicion_PID.Set();
                }catch (Exception ex)
                {
                    Console.WriteLine($"Error in PID computation thread: {ex.Message}");
                }
                Thread.Sleep(100);
            }
        });

        Stopwatch stopwatch = new Stopwatch(); // Create a stopwatch to measure the time elapsed       

        // For each temperature range, run the PID controller until the specified duration is reached
        for (int i = 0; i < temperatureRanges.Length; i++)
        {
            Console.WriteLine($"Range: {i}");
            Data.targetTemperature = (temperatureRanges[i].MinTemp + temperatureRanges[i].MaxTemp) / 2.0; // Calculate the target temperature as the average of the minimum and maximum temperature of the range
            Data.temperaturaMaxima = temperatureRanges[i].MaxTemp;
            Data.temperaturaMinima = temperatureRanges[i].MinTemp;
            Data.temp_structure.temperatures.Add(double.Parse(Data.temp_act));
            condicion.Set();
            stopwatch.Start(); // Start the stopwatch

            while (stopwatch.ElapsedMilliseconds < temperatureRanges[i].RangeTimeInMilliseconds)
            {
                ControlarRelay(relayBombilla, relayPlaca, (int)Data.output, 60, 2000); // Applying the PID controller output to the system.
            }
            pidController.Reset();
            stopwatch.Stop();
            stopwatch.Reset();
        }

        total_time_in_range += timeController.TimeInRangeInMilliseconds;
        total_time_out_of_range += timeController.TimeOutOfRangeInMilliseconds;
        Data.time_in_range_temp = (timeController.TimeInRangeInMilliseconds / 1000);

        Console.WriteLine("\n\nTiempo dentro del rango " + (((double)timeController.TimeInRangeInMilliseconds / 1000)) + " s de " + total_time / 1000 + " s");
        Console.WriteLine("Tiempo fuera del rango " + ((double)total_time_out_of_range / 1000) + " s de " + total_time / 1000 + " s");
        Console.WriteLine($"Debug Output :{timeController.LastRegisterTempDebug}");
        Console.WriteLine("RONDA TERMINADA\n\n");
    }

    private void ControlarRelay(Relay relayBombilla, Relay relayPlaca, int intensidad, int intensityBreakpoint, int periodoTiempo)
    {
        Console.WriteLine("Intensidad: {0}, IntensityBreakpoint: {1}, Periodo: {2}", intensidad, intensityBreakpoint, periodoTiempo);
        
        if (intensidad <= intensityBreakpoint)
        {
            // Código de enfriamiento
            int tiempoEncendido = (intensidad - intensityBreakpoint) * 100 / (120 - intensityBreakpoint) * periodoTiempo / (-100);
            Console.WriteLine("❄️ ENFRIANDO: Tiempo encendido del sistema de enfriamiento (peltier): {0}", tiempoEncendido);
            relayPlaca.IsOn = false;
            relayBombilla.IsOn = true;
            Thread.Sleep(tiempoEncendido);
            if(tiempoEncendido != periodoTiempo)
            {
                relayPlaca.IsOn = true;
            }
            Thread.Sleep(periodoTiempo - tiempoEncendido);
        }
        else
        {
            // Código de calentamiento
            int tiempoEncendido = (intensidad - intensityBreakpoint) * 100 / (120 - intensityBreakpoint) * periodoTiempo / 100;
            Console.WriteLine("🔥 CALENTANDO: Tiempo encendido del sistema de calentamiento (bombilla): {0}", tiempoEncendido);
            relayPlaca.IsOn = true;
            relayBombilla.IsOn = false;
            Thread.Sleep(tiempoEncendido);
            if(tiempoEncendido !=  periodoTiempo)
            {
                relayBombilla.IsOn = true;
            }
            Thread.Sleep(periodoTiempo - tiempoEncendido);
        }
        Console.WriteLine("Current temperature: {0}", Data.temp_act);
    }
}
