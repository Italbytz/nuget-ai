using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests;

[TestClass]
public class LensesTests : RealTests
{
    private readonly IDataView _data;
    private readonly IDataset _dataset;
    private LookupMap<uint>[] _lookupData = new[]
    {
        new LookupMap<uint>(1),
        new LookupMap<uint>(2),
        new LookupMap<uint>(3)
    };

    public LensesTests()
    {
        _dataset = Italbytz.AI.ML.UciDatasets.Data.Lenses;
        _data = _dataset.DataView;
        LogFile = $"log_{GetType().Name}";
    }
    
    [TestMethod]
    public void TestFlRwMacroRuntime()
    {
        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
            10000);
        Benchmark("Lenses_FlRwMacro",trainer,_data,_data,_lookupData);
    }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(10);
        
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);

        Assert.IsTrue(metrics.MacroAccuracy > 0.59);
        Assert.IsTrue(metrics.MacroAccuracy < 0.61);
    }

    protected override EstimatorChain<ITransformer?> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
            {
                new InputOutputColumnPair(@"age", @"age"),
                new InputOutputColumnPair(@"spectacle_prescription",
                    @"spectacle_prescription"),
                new InputOutputColumnPair(@"astigmatic", @"astigmatic")
            })
            .Append(mlContext.Transforms.Concatenate(@"Features", @"age",
                @"spectacle_prescription", @"astigmatic"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                @"class", keyData: lookupIdvMap))
            .Append(trainer);

        return pipeline;
    }
}