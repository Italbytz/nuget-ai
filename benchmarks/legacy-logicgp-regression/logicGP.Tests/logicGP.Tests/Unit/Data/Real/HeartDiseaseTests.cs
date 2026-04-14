using Italbytz.AI;
using Italbytz.AI.ML.LogicGp;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace logicGP.Tests.Unit.Data.Real;

[TestClass]
public class HeartDiseaseTests : RealTests
{
    private readonly IDataView _data;

    private readonly LookupMap<uint>[] _lookupData =
    [
        new(0),
        new(1),
        new(2),
        new(3),
        new(4)
    ];

    private readonly IDataset _dataset;

    public HeartDiseaseTests()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        _dataset = Italbytz.AI.ML.UciDatasets.Data.HeartDisease;
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
            new LogicGpFlcwMacroMulticlassTrainer<QuinaryClassificationOutput>(
                10000);
        SimulateFlRw(trainer, _data, _lookupData);
    }

    [TestMethod]
    //[TestCategory("FixedSeed")]
    public void TestFlRw()
    {
        ThreadSafeRandomNetCore.Seed = 42;

        var trainer = new LogicGpFlcwMacroMulticlassTrainer<QuinaryClassificationOutput>(10);
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var testResults = TestFlRw(trainer, _data, _data, _lookupData);
        var metrics = mlContext.MulticlassClassification
            .Evaluate(testResults);


        Assert.IsTrue(metrics.MacroAccuracy > 0.24);
        Assert.IsTrue(metrics.MacroAccuracy < 0.25);
    }


    protected override EstimatorChain<ITransformer> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
            {
                new InputOutputColumnPair(@"age", @"age"),
                new InputOutputColumnPair(@"sex", @"sex"),
                new InputOutputColumnPair(@"cp", @"cp"),
                new InputOutputColumnPair(@"trestbps", @"trestbps"),
                new InputOutputColumnPair(@"chol", @"chol"),
                new InputOutputColumnPair(@"fbs", @"fbs"),
                new InputOutputColumnPair(@"restecg", @"restecg"),
                new InputOutputColumnPair(@"thalach", @"thalach"),
                new InputOutputColumnPair(@"exang", @"exang"),
                new InputOutputColumnPair(@"oldpeak", @"oldpeak"),
                new InputOutputColumnPair(@"slope", @"slope"),
                new InputOutputColumnPair(@"ca", @"ca"),
                new InputOutputColumnPair(@"thal", @"thal")
            })
            .Append(mlContext.Transforms.NormalizeBinning(new[]
            {
                new InputOutputColumnPair(@"age", @"age"),
                new InputOutputColumnPair(@"trestbps", @"trestbps"),
                new InputOutputColumnPair(@"chol", @"chol"),
                new InputOutputColumnPair(@"thalach", @"thalach"),
                new InputOutputColumnPair(@"oldpeak", @"oldpeak"),
                new InputOutputColumnPair(@"ca", @"ca")
            }, maximumBinCount: 4))
            .Append(mlContext.Transforms.Concatenate(@"Features", @"age",
                @"sex", @"cp", @"trestbps", @"chol", @"fbs", @"restecg",
                @"thalach", @"exang", @"oldpeak", @"slope", @"ca", @"thal"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                @"num", keyData: lookupIdvMap))
            .Append(trainer);

        return pipeline;
    }
}