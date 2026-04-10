using System;

namespace Italbytz.AI.Evolutionary.Fitness;

public interface IFitnessValue : IComparable<IFitnessValue>, ICloneable
{
    bool IsDominating(IFitnessValue otherFitnessValue);

    double ConsolidatedValue { get; }
}
