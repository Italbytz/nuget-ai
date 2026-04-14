using System.Collections.Generic;
using Italbytz.AI.ML.LogicGp.Internal;
using Italbytz.AI;
using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;

internal abstract class RunStrategy(
    int generations,
    double minMaxWeight = 0.0,
    int folds = 5,
    int maxTime = 60)
    : CommonRunStrategy
{
    public global::Italbytz.AI.Evolutionary.Selection.IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    protected override (
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
        RunEvolutionary(IDataView input,
            Dictionary<float, int>[] featureValueMappings,
            Dictionary<uint, int> labelMapping)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var cvResults = mlContext.Data.CrossValidationSplit(input, numberOfFolds: folds);
        var individualLists =
            new global::Italbytz.AI.Evolutionary.Individuals.IIndividualList[folds];
        var foldIndex = 0;

        foreach (var fold in cvResults)
        {
            individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            foldIndex++;
        }

        var chosenIndividual = SelectionStrategy.Process(individualLists);
        var allIndividuals =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var list in individualLists)
            allIndividuals.AddRange(list);

        return (chosenIndividual, allIndividuals);
    }
}