using System.Diagnostics;
using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests;

[TestClass]
public class NationalPollTests : RealTests
{
    private readonly IDataView _data;
    private readonly IDataset _dataset;

    private readonly LookupMap<uint>[] _lookupData =
    [
        new(1),
        new(2),
        new(3)
    ];

    public NationalPollTests()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        _dataset = Italbytz.AI.ML.UciDatasets.Data.NPHA;
        _data = _dataset.DataView;
        LogFile = $"log_{GetType().Name}";
        SaveCvSplit(_data, GetType().Name);
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

    [TestMethod]
    public void TestFlRwMacroRuntime()
    {
        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
            10000);
        Benchmark("NPHA_FlRwMacro",trainer,_data,_data,_lookupData);
    }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public void TestFlRwMacro()
    {
        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
            10);
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(testResults);

        Assert.IsTrue(metrics.MacroAccuracy > 0.35);
        Assert.IsTrue(metrics.MacroAccuracy < 0.36);
    }


    protected override IEstimator<ITransformer?> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        return _dataset.BuildPipeline(mlContext, 
            trainer, ScenarioType.Classification,ProcessingType.FeatureBinningAndCustomLabelMapping);
    }
}