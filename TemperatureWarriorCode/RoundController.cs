using System;
using System.Diagnostics;
using Meadow.Foundation.Relays;
using System.Threading;
using TemperatureWarriorCode;


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
        Console.WriteLine("Starting the operations (PID and relayController)...");
        // Define the PID controller gains (kp, ki, kd). TODO: The gains should be tuned based on the system requirements.
        double kp = 0.8;
        double ki = 0.2;
        double kd = 0.001;
        // Create a PID controller with the specified gains (kp, ki, kd). TODO: The gains should be tuned based on the system requirements.
        pidController = new PIDController(kp, ki, kd);

        pidController.Reset(); // Reset the PID controller

        Stopwatch stopwatch = new Stopwatch(); // Create a stopwatch to measure the time elapsed

        // For each temperature range, run the PID controller until the specified duration is reached
        for (int i = 0; i < temperatureRanges.Length; i++)
        {
            double targetTemperature = (temperatureRanges[i].Min + temperatureRanges[i].Max) / 2.0; // Calculate the target temperature as the average of the minimum and maximum temperature of the range
            stopwatch.Start(); // Start the stopwatch

             // Se lanza un hilo que esté continuamente calculando la salida del control PID
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    // Calcular la salida del control PID
                    pidController.Compute(targetTemperature);
                    // Obtener la salida del control PID
                    int output = (int)Data.output;
                    Console.WriteLine("PID Output: " + output);
                    // Esperar un tiempo antes de volver a calcular del tiempo que tarda el sensor en actualizar la temperatura.
                    // TODO - Este tiempo debe ser ajustado según la frecuencia de actualización del sensor.
                    Thread.Sleep(1000);
                }
            });
            // Iniciamos el hilo de cálculo del control PID
            thread.Start();

            // Se activa / desactiva el relay de la bombilla y la placa de Peltier según la salida del control PID
            while (stopwatch.Elapsed.TotalSeconds < temperatureRanges[i].Duration) // While the elapsed time is less than the duration of the specific range (in seconds)
            {
                // TODO: Adapt the parameters of the ControlarRelay method to the specific system requirements.
                ControlarRelay(relayBombilla, relayPlaca, (int)Data.output, 50, 1000); // Applying the PID controller output to the system.
            }
            stopwatch.Stop();
            stopwatch.Reset();
        }
    }

    private void ControlarRelay(Relay relayBombilla, Relay relayPlaca, int intensidad, int intensityBreakpoint, int periodoTiempo)
    {
        // Intensity BreackPoint es una variable que muestra en que rango deja de enfriar y empieza a calentar
        if (intensidad >= 0 && intensidad <= intensityBreakpoint)
        {
            // Código de enfriamiento
            Console.WriteLine("Enfriando... Encendiendo el relay de la placa Peltier y apagando el de la bombilla.");
            // Calculamos el tiempo de encendido proporcional a la intensidad
            int tiempoEncendido = intensidad * (100/intensityBreakpoint) * periodoTiempo / 100;
            // Encendemos el relay de la placa de Peltier para enfriar y apagamos la bombilla
            relayPlaca.IsOn = true;
            relayBombilla.IsOn = false;
            Console.WriteLine("Tiempo de encendido: " + tiempoEncendido);
            // Esperar el tiempo de encendido
            Thread.Sleep(tiempoEncendido);

            int tiempoApagado = periodoTiempo - tiempoEncendido;
            // Apagar el relay el tiempo proporcional de apagado
            relayPlaca.IsOn = false;
            Console.WriteLine("Tiempo de apagado: " + tiempoApagado);
            Thread.Sleep(tiempoApagado);
        }
        else if (intensidad >= intensityBreakpoint && intensidad <= 100)
        {
            // Código de calentamiento
            Console.WriteLine("Calentando... Encendiendo el relay de la bombilla y apagando el de la placa Peltier.");
            // Calculamos el tiempo de encendido proporcional a la intensidad
            int tiempoEncendido = intensidad * (1-(100 / intensityBreakpoint)) * periodoTiempo / 100;
            // Encendemos el relay de la bombilla y apagamos placa de Peltier
            relayBombilla.IsOn = true;
            relayPlaca.IsOn = false;
            Console.WriteLine("Tiempo de encendido: " + tiempoEncendido);
            // Esperar el tiempo de encendido
            Thread.Sleep(tiempoEncendido);

            int tiempoApagado = periodoTiempo - tiempoEncendido;
            // Apagar el relay el tiempo proporcional de apagado
            relayBombilla.IsOn = false;
            Console.WriteLine("Tiempo de apagado: " + tiempoApagado);
            Thread.Sleep(tiempoApagado);
        }
        else
        {
            Console.WriteLine("Error: La intensidad debe estar en el rango de 0 a 100.");
        }
    }
}
