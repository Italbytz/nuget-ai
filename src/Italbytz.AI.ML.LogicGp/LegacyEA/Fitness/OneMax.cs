using System.Linq;
using Italbytz.EA.Searchspace;

namespace Italbytz.AI.ML.LogicGp.Internal.Fitness;

internal class OneMax : global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction
{
    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue Evaluate(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        return EvaluateCore(((BitStringGenotype)individual.Genotype).BitArray);
    }

    private static SingleFitnessValue EvaluateCore(System.Collections.BitArray bitArray)
    {
        var count = bitArray.Cast<bool>().Count(bit => bit);
        return new SingleFitnessValue(count);
    }

    public int NumberOfObjectives { get; }
}