using System;

public class TemperatureRange
{
    public double Min { get; set; }
    public double Max { get; set; }
    public int Duration { get; set; }

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

        Min = min;
        Max = max;
        Duration = duration;
    }
}
