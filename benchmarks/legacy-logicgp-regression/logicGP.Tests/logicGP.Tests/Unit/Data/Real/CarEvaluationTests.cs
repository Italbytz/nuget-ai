using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests;

[TestClass]
public class CarEvaluationTests : RealTests
{
    private readonly IDataView _data;
    private readonly IDataset _dataset;
    private LookupMap<string>[] _lookupData = new[]
    {
        new LookupMap<string>("unacc"),
        new LookupMap<string>("acc"),
        new LookupMap<string>("good"),
        new LookupMap<string>("vgood")
    };

    public CarEvaluationTests()
    {
        _dataset = Italbytz.AI.ML.UciDatasets.Data.CarEvaluation;
        _data = _dataset.DataView;
        LogFile = $"log_{GetType().Name}";
    }

    [TestMethod]
    public void TestFlRwMacroRuntime()
    {
        var trainer = new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
            10000);
        Benchmark("Car_FlRwMacro",trainer,_data,_data,_lookupData);
    }
    
    [TestMethod]
    [TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer = new LogicGpFlcwMacroMulticlassTrainer<QuaternaryClassificationOutput>(10);

        
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);

        Assert.IsTrue(metrics.MacroAccuracy > 0.32);
        Assert.IsTrue(metrics.MacroAccuracy < 0.33);
    }

    protected override EstimatorChain<ITransformer?> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;

        var buyingLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "low" },
            new CategoryLookupMap { Value = 1f, Category = "med" },
            new CategoryLookupMap { Value = 2f, Category = "high" },
            new CategoryLookupMap { Value = 3f, Category = "vhigh" }
        };
        var buyingLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(buyingLookupData);

        var maintLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "low" },
            new CategoryLookupMap { Value = 1f, Category = "med" },
            new CategoryLookupMap { Value = 2f, Category = "high" },
            new CategoryLookupMap { Value = 3f, Category = "vhigh" }
        };
        var maintLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(maintLookupData);

        var doorsLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "two" },
            new CategoryLookupMap { Value = 1f, Category = "three" },
            new CategoryLookupMap { Value = 2f, Category = "four" },
            new CategoryLookupMap { Value = 3f, Category = "fiveormore" }
        };
        var doorsLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(doorsLookupData);

        var personsLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "two" },
            new CategoryLookupMap { Value = 1f, Category = "four" },
            new CategoryLookupMap { Value = 2f, Category = "more" }
        };
        var personsLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(personsLookupData);

        var lugBootLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "small" },
            new CategoryLookupMap { Value = 1f, Category = "med" },
            new CategoryLookupMap { Value = 2f, Category = "big" }
        };
        var lugBootLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(lugBootLookupData);

        var safetyLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "low" },
            new CategoryLookupMap { Value = 1f, Category = "med" },
            new CategoryLookupMap { Value = 2f, Category = "high" }
        };
        var safetyLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(safetyLookupData);

        var pipeline =
            mlContext.Transforms.Conversion.MapValue("buying",
                    buyingLookupIdvMap, buyingLookupIdvMap.Schema["Category"],
                    buyingLookupIdvMap.Schema[
                        "Value"], "buying")
                .Append(mlContext.Transforms.Conversion.MapValue("maint",
                    maintLookupIdvMap, maintLookupIdvMap.Schema["Category"],
                    maintLookupIdvMap.Schema[
                        "Value"], "maint"))
                .Append(mlContext.Transforms.Conversion.MapValue("doors",
                    doorsLookupIdvMap, doorsLookupIdvMap.Schema["Category"],
                    doorsLookupIdvMap.Schema[
                        "Value"], "doors"))
                .Append(mlContext.Transforms.Conversion.MapValue("persons",
                    personsLookupIdvMap, personsLookupIdvMap.Schema["Category"],
                    personsLookupIdvMap.Schema[
                        "Value"], "persons"))
                .Append(mlContext.Transforms.Conversion.MapValue("lug_boot",
                    lugBootLookupIdvMap, lugBootLookupIdvMap.Schema["Category"],
                    lugBootLookupIdvMap.Schema[
                        "Value"], "lug_boot"))
                .Append(mlContext.Transforms.Conversion.MapValue("safety",
                    safetyLookupIdvMap, safetyLookupIdvMap.Schema["Category"],
                    safetyLookupIdvMap.Schema[
                        "Value"], "safety"))
                .Append(mlContext.Transforms.Concatenate(@"Features", @"buying",
                    @"maint", @"doors", @"persons", @"lug_boot", @"safety"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                    @"class", keyData: lookupIdvMap))
                .Append(trainer);

        return pipeline;
    }
}