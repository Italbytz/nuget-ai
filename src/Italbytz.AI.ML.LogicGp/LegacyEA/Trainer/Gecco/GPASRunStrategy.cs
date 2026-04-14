using System.Threading.Tasks;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;

internal class GPASRunStrategy(int generations, double minMaxWeight = 0.0)
    : RunStrategy(generations, minMaxWeight)
{
    protected override Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new RandomInitialization { Size = 2 },
            weighting: Weighting.ComputedBinary, generations: generations,
            minMaxWeight: minMaxWeight);
    }
}