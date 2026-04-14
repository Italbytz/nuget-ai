using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests.Unit.Data.Real;

[TestClass]
public class WineQualityTests : RealTests
{
    private readonly IDataView _data;

    private readonly LookupMap<uint>[] _lookupData =
    [
        //new LookupMap<uint>(0),
        //new LookupMap<uint>(1),
        //new LookupMap<uint>(2),
        new(3),
        new(4),
        new(5),
        new(6),
        new(7),
        new(8),
        new(9)
        //new LookupMap<uint>(10)
    ];

    private readonly IDataset _dataset;

    public WineQualityTests()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        _dataset = Italbytz.AI.ML.UciDatasets.Data.WineQuality;
        _data = _dataset.DataView;
        LogFile = $"log_{GetType().Name}";
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
            new LogicGpFlcwMacroMulticlassTrainer<
                SeptenaryClassificationOutput>(10000);
        SimulateFlRw(trainer, _data, _lookupData);
    }
    
    [TestMethod]
    //[TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<
                SeptenaryClassificationOutput>(10);

        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);


        Assert.IsTrue(metrics.MacroAccuracy > 0.16);
        Assert.IsTrue(metrics.MacroAccuracy < 0.17);
    }


    protected override EstimatorChain<ITransformer> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
            {
                new InputOutputColumnPair(@"fixed_acidity", @"fixed_acidity"),
                new InputOutputColumnPair(@"volatile_acidity",
                    @"volatile_acidity"),
                new InputOutputColumnPair(@"citric_acid", @"citric_acid"),
                new InputOutputColumnPair(@"residual_sugar", @"residual_sugar"),
                new InputOutputColumnPair(@"chlorides", @"chlorides"),
                new InputOutputColumnPair(@"free_sulfur_dioxide",
                    @"free_sulfur_dioxide"),
                new InputOutputColumnPair(@"total_sulfur_dioxide",
                    @"total_sulfur_dioxide"),
                new InputOutputColumnPair(@"density", @"density"),
                new InputOutputColumnPair(@"pH", @"pH"),
                new InputOutputColumnPair(@"sulphates", @"sulphates"),
                new InputOutputColumnPair(@"alcohol", @"alcohol")
            })
            .Append(mlContext.Transforms.NormalizeBinning(new[]
            {
                new InputOutputColumnPair(@"fixed_acidity", @"fixed_acidity"),
                new InputOutputColumnPair(@"volatile_acidity",
                    @"volatile_acidity"),
                new InputOutputColumnPair(@"citric_acid", @"citric_acid"),
                new InputOutputColumnPair(@"residual_sugar", @"residual_sugar"),
                new InputOutputColumnPair(@"chlorides", @"chlorides"),
                new InputOutputColumnPair(@"free_sulfur_dioxide",
                    @"free_sulfur_dioxide"),
                new InputOutputColumnPair(@"total_sulfur_dioxide",
                    @"total_sulfur_dioxide"),
                new InputOutputColumnPair(@"density", @"density"),
                new InputOutputColumnPair(@"pH", @"pH"),
                new InputOutputColumnPair(@"sulphates", @"sulphates"),
                new InputOutputColumnPair(@"alcohol", @"alcohol")
            }, maximumBinCount: 4))
            .Append(mlContext.Transforms.Concatenate(@"Features",
                @"fixed_acidity", @"volatile_acidity", @"citric_acid",
                @"residual_sugar", @"chlorides", @"free_sulfur_dioxide",
                @"total_sulfur_dioxide", @"density", @"pH", @"sulphates",
                @"alcohol"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                @"quality", keyData: lookupIdvMap))
            .Append(trainer);

        return pipeline;
    }
}