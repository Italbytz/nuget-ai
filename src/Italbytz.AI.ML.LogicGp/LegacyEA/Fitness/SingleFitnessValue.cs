using System;

namespace Italbytz.AI.ML.LogicGp.Internal.Fitness;

internal class SingleFitnessValue(double fitness)
    : global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue
{
    public int CompareTo(
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? other)
    {
        return Compare(this, other);
    }

    private int Compare(
        SingleFitnessValue fitnessValue,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? other)
    {
        if (other is null) return 1;
        if (other is not SingleFitnessValue otherFitnessValue) return -1;
        return fitnessValue.Fitness.CompareTo(otherFitnessValue.Fitness);
    }

    public object Clone()
    {
        return new SingleFitnessValue(fitness);
    }

    public bool IsDominating(
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue otherFitnessValue)
    {
        if (otherFitnessValue is not SingleFitnessValue other)
            throw new ArgumentException("Expected fitness value of type SingleFitnessValue");
        return Fitness > other.Fitness;
    }

    public double ConsolidatedValue => Fitness;

    public double Fitness { get; } = fitness;
    
    public override string ToString()
    {
        return Fitness.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}