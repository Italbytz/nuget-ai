using System;

namespace Italbytz.EA.StoppingCriterion;

internal class FitnessBound : global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion
{
    public bool IsMet()
    {
        Console.WriteLine(
            "FitnessBound stopping criterion is not implemented.");
        return false;
    }
}