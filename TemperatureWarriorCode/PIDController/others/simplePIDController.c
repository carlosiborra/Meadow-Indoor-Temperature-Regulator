const int INPUT_PIN = A0; // Temperature Sensor. Substitute with corresponding PIN
const int OUTPUT_PIN = DD3; // Cooling Fan / Heater. Substitute with corresponding PIN

double dt, last_time;
double integral, previous, output = 0;
double kp, ki, kd; // PID Constants.
double target_temperature = 20.5; // Target Temperature: Mean value of the temperature range. Substitute with the desired value

int min_temperature = 0; // Min value of the temperature sensor
int max_temperature = 55; // Max value of the temperature sensor
int min_precentage = 0; // Min value of the output (cooling fan / heater)
int max_percentage = 100; // Max value of the output (cooling fan / heater)

void setup()
{
  // THE VALUES OF THESE 3 CONSTANTS MUST BE TUNED TO THE SPECIFIC SYSTEM. THERE ARE METHODS TO TUNE THEM.
  // SOME SYSTEMS MAY NOT NEED ALL OF THEM. IT DEPENDS ON THE SYSTEM. PID IS A GENERIC ALGORITHM (PD OR PI CONTROLLERS MIGHT BE USED TOO)
  kp = 0.8; // Gain value of the proportional term. Indicates how much does the output change with respect to the error.
  ki = 0.20; // Gain value of the integral term. Indicates how much does the output change with respect to integral term .
  kd = 0.001; // Gain value of the derivative term. Indicates how much does the output change with respect to the derivative term.

  last_time = 0;
  Serial.begin(9600); 
  analogWrite(OUTPUT_PIN, 0);
  for(int i = 0; i < 50; i++)
  {
    Serial.print(target_temperature);
    Serial.print(",");
    Serial.println(0);
    delay(100); 
  }
  delay(100);
}

void loop()
{
  double now = millis(); // Time in milliseconds
  dt = (now - last_time)/1000.00; // differential of time in seconds
  last_time = now; // Update the last time

  double actual = map(analogRead(INPUT_PIN), min_temperature, max_temperature, min_precentage, max_percentage); // Read the temperature sensor
  double error = target_temperature - actual; // Calculate the error
  output = pid(error); // Calculate the output using the PID algorithm

  analogWrite(OUTPUT_PIN, output); // Write the output to the cooling fan / heater pin (system used to cool or heat the environment) --> PWM pulse/ percentage of intensity

  // target_temperature VS Actual
  Serial.print(target_temperature);
  Serial.print(",");
  Serial.println(actual);

  // Error
  //Serial.println(error);

  delay(10); // Delay of 10 milliseconds
}

double pid(double error)
{
  double proportional = error; // Proportional term of the PID algorithm. It is the error itself
  integral += error * dt; // Integral term of the PID algorithm. It is the sum of the errors over time (integral of the error --> area (small rectangle) under the curve of the error over time)
  double derivative = (error - previous) / dt; // Derivative term of the PID algorithm. It is the rate of change of the error over time (derivative of the error --> slope of the error over time)
  previous = error;
  double output = (kp * proportional) + (ki * integral) + (kd * derivative);
  return output;
}