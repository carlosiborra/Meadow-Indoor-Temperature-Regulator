// HOW TO USE THIS PID CONTROLLER

using System;
using System.Diagnostics;

// Create a PID controller with the specified gains (kp, ki, kd). The gains should be tuned based on the system requirements.
PIDController controller = new PIDController(0.8, 0.2, 0.001);

// Define the temperature ranges and their corresponding durations. In the project, obtain these values from the frontend (server)
double temp_min_1 = 16.0; // Minimum temperature for range 1
double temp_max_1 = 18.0; // Maximum temperature for range 1
double temp_min_2 = 18.0; // Minimum temperature for range 2
double temp_max_2 = 20.0; // Maximum temperature for range 2
double temp_min_3 = 20.0; // Minimum temperature for range 3
double temp_max_3 = 22.0; // Maximum temperature for range 3
double temp_min_4 = 22.0; // Minimum temperature for range 4
double temp_max_4 = 24.0; // Maximum temperature for range 4


int[] duracion_rangos = { 6, 2, 3, 15, 7 }; // array of duration of each range

// array of temperature ranges
double[][] rangosTemperaturas =
        {
            new double[] { temp_min_1, temp_max_1 },
            new double[] { temp_min_2, temp_max_2 },
            new double[] { temp_min_3, temp_max_3 },
            new double[] { temp_min_4, temp_max_4 }
        };

Stopwatch stopwatch = new Stopwatch(); // Create a stopwatch to measure the time elapsed

// For each temperature range, run the PID controller until the specified duration is reached
for (int i = 0; i < duracion_rangos.Length; i++) {
    double temperatura_objetivo = (rangosTemperaturas[i][0] + rangosTemperaturas[i][1]) / 2.0 ; // Calcular la temperatura objetivo como el promedio de la temperatura mínima y máxima del rango
    stopwatch.Start(); // Start the stopwatch

    while (stopwatch.Elapsed.TotalSeconds < duracion_rangos[i]) // While the elapsed time is less than the duration of the specific range (in seconds)
      {
      double actualTemperature = obtenerTemperaturaActual(); // Obtaining the current temperature. Adapt this funcion to the one used in the project
      double output = controller.Compute(actualTemperature, temperatura_objetivo); // Compute the PID controller output based on the current temperature and the target temperature
      aplicarSalidaControlador(output); // Applying the PID controller output to the system. Adapt this function to the one used in the project
      }

}
// Detener el cronómetro
stopwatch.Stop();


