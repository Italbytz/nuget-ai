using System.Collections;
using System.Linq;
using Italbytz.AI.Evolutionary.Individuals;
using Italbytz.AI.Evolutionary.SearchSpace;

namespace Italbytz.AI.Evolutionary.Fitness;

public class OneMax : IFitnessFunction
{
    public IFitnessValue Evaluate(IIndividual individual)
    {
        var genotype = individual.Genotype as BitStringGenotype
            ?? throw new ArgumentException("Expected a BitStringGenotype.", nameof(individual));

        var count = genotype.BitArray.Cast<bool>().Count(bit => bit);
        return new SingleFitnessValue(count);
    }
}
