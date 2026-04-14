using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.AI.ML.LogicGp.Internal.Trainer.Bioinformatics;
using Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;

namespace Italbytz.AI.ML.LogicGp;

public class LogicGpGpasBinaryTrainer
    : LogicGpTrainer<BinaryClassificationOutput>
{
    public LogicGpGpasBinaryTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Micro);
        RunStrategy = new GPASRunStrategy(generations);
    }
}

public class LogicGpFlcwMacroMulticlassTrainer<TOutput>
    : LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    public LogicGpFlcwMacroMulticlassTrainer(int generations, int folds = 5,
        int maxTime = 60)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Macro);
        RunStrategy = new FlcwRunStrategy(generations, folds, maxTime);
    }
}

public class LogicGpFlcwMicroMulticlassTrainer<TOutput>
    : LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    public LogicGpFlcwMicroMulticlassTrainer(int generations, int folds = 5,
        int maxTime = 60)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Micro);
        RunStrategy = new FlcwRunStrategy(generations, folds, maxTime);
    }
}

public class LogicGpRlcwMacroMulticlassTrainer<TOutput>
    : LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    public LogicGpRlcwMacroMulticlassTrainer(
        int phase1Time,
        int phase2Time,
        int maxIndividuals = 1000,
        int crossoverIndividuals = 14,
        int mutationIndividuals = 1,
        int folds = 5,
        double minMaxWeight = 0.0)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.F1, Averaging.Macro);
        RunStrategy = new RlcwRunStrategy(
            new LogicGpGraph(maxIndividuals, crossoverIndividuals,
                mutationIndividuals),
            phase1Time,
            phase2Time,
            folds,
            minMaxWeight);
    }
}

public class LogicGpRlcwMicroMulticlassTrainer<TOutput>
    : LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    public LogicGpRlcwMicroMulticlassTrainer(
        int phase1Time,
        int phase2Time,
        int maxIndividuals = 1000,
        int crossoverIndividuals = 14,
        int mutationIndividuals = 1,
        int folds = 5,
        double minMaxWeight = 0.0)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.F1, Averaging.Micro);
        RunStrategy = new RlcwRunStrategy(
            new LogicGpGraph(maxIndividuals, crossoverIndividuals,
                mutationIndividuals),
            phase1Time,
            phase2Time,
            folds,
            minMaxWeight);
    }
}