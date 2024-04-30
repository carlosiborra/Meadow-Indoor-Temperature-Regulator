using System;
public class PIDController
{
    private double kp; // Gain value of the proportional term
    private double ki; // Gain value of the integral term
    private double kd; // Gain value of the derivative term

    private double integral; // Integral term accumulator
    private double previousError; // Previous error for derivative term calculation
    private double lastTime; // Last time the PID control output was computed

    // Constructor
    public PIDController(double kp, double ki, double kd)
    {
        this.kp = kp;
        this.ki = ki;
        this.kd = kd;

        Reset();
    }

    // Method to reset the controller
    public void Reset()
    {
        integral = 0;
        previousError = 0;
        lastTime = GetCurrentTime();
    }

    // Method to get the current time in milliseconds
    private double GetCurrentTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    // Method to compute the PID control output based on the error
    public double Compute(double actualTemperature, double desiredTemperature)
    {
        double error = desiredTemperature - actualTemperature; // Error between the desired and actual temperature

        double currentTime = GetCurrentTime();
        double dt = (currentTime - lastTime) / 1000; // Time difference in seconds
        lastTime = currentTime; // Update the last time

        double proportional = error; // Proportional term
        integral += error * dt; // Integral term
        double derivative = (error - previousError) / dt; // Derivative term
        previousError = error;

        // Compute the PID output
        double output = (kp * proportional) + (ki * integral) + (kd * derivative);
        
        return output;
    }

    // Properties to get and set the PID gains
    public double Kp // Gain value of the proportional term
    {
        get { return kp; }
        set { kp = value; }
    }

    public double Ki // Gain value of the integral term
    {
        get { return ki; }
        set { ki = value; }
    }

    public double Kd // Gain value of the derivative term
    {
        get { return kd; }
        set { kd = value; }
    }
}
