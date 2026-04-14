using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;

namespace logicGP.Tests.Unit.Data.Real;

[TestClass]
public class IrisTests : RealTests
{
    private readonly IDataView _data;
    private readonly IDataset _dataset;
    
    private readonly LookupMap<string>[] _lookupData =
    [
        new("Iris-setosa"),
        new("Iris-versicolor"),
        new("Iris-virginica")
    ];

    public IrisTests()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        _dataset = Italbytz.AI.ML.UciDatasets.Data.Iris;
        _data = _dataset.DataView;
        LogFile = $"log_{GetType().Name}";
        //SaveCvSplit(_data, GetType().Name);
    }

    [TestCleanup]
    public void TearDown()
    {
        LogWriter?.Dispose();
    }

    [TestMethod]
    public void SimulateFlRwMacro()
    {
        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
                10000);
        SimulateFlRw(trainer, _data, _lookupData);
    }
    
    private void PrintFeaturesAndLabels(IDataView predictions)
    {
        var excerpt = predictions.GetDataExcerpt("Label");
        var features = excerpt.Features;
        var labels = excerpt.Labels;
        Console.WriteLine("private readonly string[][] _features = [");
        foreach (var feature in features)
            Console.WriteLine(
                $"    [ {string.Join(", ", feature.Select(f => f == 0.0 ? "\"1\"": Math.Abs(f - 1f/3f) < 0.1 ? "\"2\"" : Math.Abs(f - 2f/3f) < 0.1 ? "\"3\"" : "\"4\""))} ],");
        Console.WriteLine("];");

        Console.WriteLine("private readonly string[] _labels = [");
        foreach (var label in labels) Console.WriteLine($"    \"{label}\",");
        Console.WriteLine("];");
    }
    
    [TestMethod]
    [TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(10);


        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        PrintFeaturesAndLabels(testResults);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);


        Assert.IsTrue(metrics.MacroAccuracy > 0.33);
        Assert.IsTrue(metrics.MacroAccuracy < 0.34);
    }

    protected override IEstimator<ITransformer> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        return _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext,  trainer,ScenarioType.Classification,ProcessingType.FeatureBinningAndCustomLabelMapping);
    }
}