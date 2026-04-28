using System.Threading.Tasks;
using Italbytz.EA.Initialization;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;

internal class FlcwRunStrategy(
    int generations,
    int folds = 5,
    int maxTime = 60,
    double minMaxWeight = 0.0)
    : RunStrategy(generations, minMaxWeight, folds, maxTime)
{
    protected override Task<(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new CompleteInitialization(),
            generations: generations, maxTime: maxTime, minMaxWeight: minMaxWeight);
    }
}