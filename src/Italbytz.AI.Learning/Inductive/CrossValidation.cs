using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Learning.Framework;

namespace Italbytz.AI.Learning.Inductive;

public class CrossValidation(double minErrT) : ICrossValidation
{
    public IParameterizedLearner CrossValidationWrapper(IParameterizedLearner learner, int k, IDataSet examples)
    {
        var errT = new List<double>();
        var errV = new List<double>();
        for (var size = 0; ; size++)
        {
            var crossValidationResult = ConductCrossValidation(learner, size, k, examples);
            errT.Add(crossValidationResult[0]);
            errV.Add(crossValidationResult[1]);
            if (!HasConverged(errT[size]))
            {
                continue;
            }

            var bestSize = errV.IndexOf(errV.Min());
            learner.Train(bestSize, examples);
            return learner;
        }
    }

    private bool HasConverged(double trainingError)
    {
        return trainingError < minErrT;
    }

    private double[] ConductCrossValidation(IParameterizedLearner learner, int size, int k, IDataSet examples)
    {
        var foldError = new double[2];
        for (var fold = 0; fold < k; fold++)
        {
            var partition = Partition(examples, fold, k);
            var trainingSet = partition[0];
            var validationSet = partition[1];

            learner.Train(size, trainingSet);
            foldError[0] += ErrorRate(learner.Test(trainingSet));
            foldError[1] += ErrorRate(learner.Test(validationSet));
        }

        foldError[0] /= k;
        foldError[1] /= k;
        return foldError;
    }

    private static double ErrorRate(int[] test)
    {
        return Math.Abs((double)(test[0] - test[1])) / 100;
    }

    private static IDataSet[] Partition(IDataSet examples, int fold, int k)
    {
        var indices = Enumerable.Range(0, examples.Examples.Count)
            .OrderBy(_ => ThreadSafeRandomNetCore.Shared.Next())
            .ToList();

        var trainingExamples = new List<IExample>();
        var validationExamples = new List<IExample>();
        for (var i = 0; i < indices.Count; i++)
        {
            if (i < k)
            {
                validationExamples.Add(examples.Examples[indices[i]]);
            }
            else
            {
                trainingExamples.Add(examples.Examples[indices[i]]);
            }
        }

        var trainingSet = new DataSet(examples.Specification) { Examples = trainingExamples };
        var validationSet = new DataSet(examples.Specification) { Examples = validationExamples };
        return [trainingSet, validationSet];
    }
}
