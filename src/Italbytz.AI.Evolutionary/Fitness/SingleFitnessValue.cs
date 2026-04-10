using System;
using System.Globalization;

namespace Italbytz.AI.Evolutionary.Fitness;

public class SingleFitnessValue(double fitness) : IFitnessValue
{
    public double Fitness { get; } = fitness;

    public double ConsolidatedValue => Fitness;

    public int CompareTo(IFitnessValue? other)
    {
        return other is SingleFitnessValue otherFitnessValue
            ? Fitness.CompareTo(otherFitnessValue.Fitness)
            : 1;
    }

    public object Clone()
    {
        return new SingleFitnessValue(Fitness);
    }

    public bool IsDominating(IFitnessValue otherFitnessValue)
    {
        if (otherFitnessValue is not SingleFitnessValue other)
        {
            throw new ArgumentException("Expected fitness value of type SingleFitnessValue", nameof(otherFitnessValue));
        }

        return Fitness > other.Fitness;
    }

    public override string ToString()
    {
        return Fitness.ToString(CultureInfo.InvariantCulture);
    }
}
