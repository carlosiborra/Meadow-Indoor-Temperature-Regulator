using System;

public class TemperatureRange
{
    public double Min { get; set; }
    public double Max { get; set; }
    public int Duration { get; set; }

    private static readonly double max_allowed_temp = 55.0; // In ºC
    private static readonly double max_temp_comp = 38.0; // In ºC
    private static readonly double min_temp_comp = 12.0; // In ºC


    public TemperatureRange(double min, double max, int duration)
    {
        
        if (min >= max)
        {
            throw new ArgumentException("Min temperature should be less than max temperature.");
        }

        if (duration <= 0)
        {
            throw new ArgumentException("Duration should be greater than zero.");
        }

        if (checkTemperature(min) && checkTemperature(max))
        {
            Min = min;
            Max = max;
            Duration = duration;
        }
    }

    public bool checkTemperature(double temperature)
    {
        if (temperature > max_allowed_temp)
        {
            throw new ArgumentException($"Max temperature should be less than {max_allowed_temp}ºC.");
        }

        if (temperature < min_temp_comp || temperature > max_temp_comp)
        {
            throw new ArgumentException($"Temperature range should be between {min_temp_comp}ºC and {max_temp_comp}ºC.");
        }   
        return true;
    }
}
