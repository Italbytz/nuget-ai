using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests;

[TestClass]
public class SolarflareTests : RealTests
{
    private readonly IDataView _data;
    private readonly IDataset _dataset;
    LookupMap<uint>[] _lookupData = new[]
    {
        new LookupMap<uint>(0),
        new LookupMap<uint>(1),
        new LookupMap<uint>(2),
        new LookupMap<uint>(3),
        new LookupMap<uint>(4),
        new LookupMap<uint>(5),
        new LookupMap<uint>(6),
        new LookupMap<uint>(8)
    };
    public SolarflareTests()
    {
        _dataset = Italbytz.AI.ML.UciDatasets.Data.SolarFlare;
        _data = _dataset.DataView;
    }

    [TestMethod]
    public void TestFlRwMacroRuntime()
    {
        var trainer = new LogicGpFlcwMacroMulticlassTrainer<OctonaryClassificationOutput>(
            10000);
        Benchmark("SF",trainer,_data,_data,_lookupData);
    }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<OctonaryClassificationOutput>(
                10);

        
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);

        Assert.IsTrue(metrics.MacroAccuracy > 0.16);
        Assert.IsTrue(metrics.MacroAccuracy < 0.17);
    }

    protected override EstimatorChain<ITransformer?> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;

        var zurichLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "A" },
            new CategoryLookupMap { Value = 1f, Category = "B" },
            new CategoryLookupMap { Value = 2f, Category = "C" },
            new CategoryLookupMap { Value = 3f, Category = "D" },
            new CategoryLookupMap { Value = 4f, Category = "E" },
            new CategoryLookupMap { Value = 5f, Category = "F" },
            new CategoryLookupMap { Value = 6f, Category = "H" }
        };
        var zurichLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(zurichLookupData);

        var spotSizeData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "X" },
            new CategoryLookupMap { Value = 1f, Category = "R" },
            new CategoryLookupMap { Value = 2f, Category = "S" },
            new CategoryLookupMap { Value = 3f, Category = "A" },
            new CategoryLookupMap { Value = 4f, Category = "H" },
            new CategoryLookupMap { Value = 5f, Category = "K" }
        };
        var spotSizeIdvMap =
            mlContext.Data.LoadFromEnumerable(spotSizeData);

        var spotDistributionData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "X" },
            new CategoryLookupMap { Value = 1f, Category = "O" },
            new CategoryLookupMap { Value = 2f, Category = "I" },
            new CategoryLookupMap { Value = 3f, Category = "C" }
        };

        var spotDistributionIdvMap =
            mlContext.Data.LoadFromEnumerable(spotDistributionData);


        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
            {
                new InputOutputColumnPair(@"activity", @"activity"),
                new InputOutputColumnPair(@"evolution", @"evolution"),
                new InputOutputColumnPair(@"previous 24 hour flare activity",
                    @"previous 24 hour flare activity"),
                new InputOutputColumnPair(@"historically-complex",
                    @"historically-complex"),
                new InputOutputColumnPair(@"became complex on this pass",
                    @"became complex on this pass"),
                new InputOutputColumnPair(@"area", @"area"),
                new InputOutputColumnPair(@"area of largest spot",
                    @"area of largest spot")
            }).Append(mlContext.Transforms.Conversion.MapValue(
                "modified Zurich class",
                zurichLookupIdvMap, zurichLookupIdvMap.Schema["Category"],
                zurichLookupIdvMap.Schema[
                    "Value"], "modified Zurich class"))
            .Append(mlContext.Transforms.Conversion.MapValue(
                "largest spot size",
                spotSizeIdvMap, spotSizeIdvMap.Schema["Category"],
                spotSizeIdvMap.Schema[
                    "Value"], "largest spot size"))
            .Append(mlContext.Transforms.Conversion.MapValue(
                "spot distribution",
                spotDistributionIdvMap,
                spotDistributionIdvMap.Schema["Category"],
                spotDistributionIdvMap.Schema[
                    "Value"], "spot distribution"))
            .Append(mlContext.Transforms.Concatenate(@"Features",
                @"modified Zurich class", @"spot distribution", @"activity",
                @"evolution", @"previous 24 hour flare activity",
                @"historically-complex", @"became complex on this pass",
                @"area", @"area of largest spot", @"largest spot size"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                @"flares", keyData: lookupIdvMap))
            .Append(trainer);

        return pipeline;
    }
}