using System.Globalization;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.LogicGp;
using logicGP.Tests.Data.Simulated;
using Microsoft.ML;

namespace logicGP.Tests.Unit.Data.Simulated;

[TestClass]
public sealed class SNPSimulationTests
{
    [TestMethod]
    public void TestSimulation1GPAS()
    {
        var trainer = new LogicGpGpasBinaryTrainer(10000);
        GPASSimulation("Simulation1", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation2GPAS()
    {
        var trainer = new LogicGpGpasBinaryTrainer(10000);
        GPASSimulation("Simulation2", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation3GPAS()
    {
        var trainer = new LogicGpGpasBinaryTrainer(10000);
        GPASSimulation("Simulation3", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation1RlRw()
    {
        var trainer =
            new LogicGpRlcwMacroMulticlassTrainer<BinaryClassificationOutput>(-5,
                300,
                folds: 5, minMaxWeight: 1.05);

        GPASSimulation("Simulation1", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation2RlRw()
    {
        var trainer =
            new LogicGpRlcwMacroMulticlassTrainer<BinaryClassificationOutput>(-6,
                100,
                folds: 5, minMaxWeight: 1.05);

        GPASSimulation("Simulation2", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestNewVersion()
    {
        Console.WriteLine("9.4.2");
    }

    [TestMethod]
    public void TestSimulation3RlRw()
    {
        var trainer =
            new LogicGpRlcwMacroMulticlassTrainer<BinaryClassificationOutput>(-4,
                300,
                folds: 5, minMaxWeight: 1.05);

        GPASSimulation("Simulation3", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation1FlRw()
    {
        var trainer =
            new LogicGpFlcwMicroMulticlassTrainer<BinaryClassificationOutput>(
                10000);

        GPASSimulation("Simulation1", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation2FlRw()
    {
        var trainer =
            new LogicGpFlcwMicroMulticlassTrainer<BinaryClassificationOutput>(
                10000);

        GPASSimulation("Simulation2", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestSimulation3FlRw()
    {
        var trainer =
            new LogicGpFlcwMicroMulticlassTrainer<BinaryClassificationOutput>(
                10000);

        GPASSimulation("Simulation3", AppDomain.CurrentDomain.BaseDirectory,
            trainer);
        // Only test successful completion
        Assert.IsTrue(true);
    }

    public void GPASSimulation(string folder, string logFolder,
        IEstimator<ITransformer> trainer)
    {
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var path = Path.Combine(logFolder,
            $"logicgpgpasacc_{timeStamp}_log.txt");
        using var logWriter = new StreamWriter(path);
        path = Path.Combine(logFolder,
            $"logicgpgpasacc_{timeStamp}.csv");
        using var writer = new StreamWriter(path);
        writer.WriteLine("\"x\"");
        var mlContext = ThreadSafeMLContext.LocalMLContext;


        var lookupData = new[]
        {
            new LookupMap<uint>(0),
            new LookupMap<uint>(1)
        };
        // Convert to IDataView
        var lookupIdvMap = mlContext.Data.LoadFromEnumerable(lookupData);


        for (var j = 1; j < 101; j++)
        {
            var trainDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"Data/Simulated/{folder}", $"SNPglm_{j}.csv");
            logWriter.WriteLine($"Training on {trainDataPath}");
            var trainData = mlContext.Data.LoadFromTextFile<SNPModelInput>(
                trainDataPath,
                ',', true);
            var testIndex = j == 100 ? 1 : j + 1;
            var testDataPath =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    $"Data/Simulated/{folder}", $"SNPglm_{testIndex}.csv");
            logWriter.WriteLine($"Testing on {testDataPath}");
            var testData = mlContext.Data.LoadFromTextFile<SNPModelInput>(
                testDataPath,
                ',', true);

            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair(@"SNP1", @"SNP1"),
                    new InputOutputColumnPair(@"SNP2", @"SNP2"),
                    new InputOutputColumnPair(@"SNP3", @"SNP3"),
                    new InputOutputColumnPair(@"SNP4", @"SNP4"),
                    new InputOutputColumnPair(@"SNP5", @"SNP5"),
                    new InputOutputColumnPair(@"SNP6", @"SNP6"),
                    new InputOutputColumnPair(@"SNP7", @"SNP7"),
                    new InputOutputColumnPair(@"SNP8", @"SNP8"),
                    new InputOutputColumnPair(@"SNP9", @"SNP9"),
                    new InputOutputColumnPair(@"SNP10", @"SNP10"),
                    new InputOutputColumnPair(@"SNP11", @"SNP11"),
                    new InputOutputColumnPair(@"SNP12", @"SNP12"),
                    new InputOutputColumnPair(@"SNP13", @"SNP13"),
                    new InputOutputColumnPair(@"SNP14", @"SNP14"),
                    new InputOutputColumnPair(@"SNP15", @"SNP15"),
                    new InputOutputColumnPair(@"SNP16", @"SNP16"),
                    new InputOutputColumnPair(@"SNP17", @"SNP17"),
                    new InputOutputColumnPair(@"SNP18", @"SNP18"),
                    new InputOutputColumnPair(@"SNP19", @"SNP19"),
                    new InputOutputColumnPair(@"SNP20", @"SNP20"),
                    new InputOutputColumnPair(@"SNP21", @"SNP21"),
                    new InputOutputColumnPair(@"SNP22", @"SNP22"),
                    new InputOutputColumnPair(@"SNP23", @"SNP23"),
                    new InputOutputColumnPair(@"SNP24", @"SNP24"),
                    new InputOutputColumnPair(@"SNP25", @"SNP25"),
                    new InputOutputColumnPair(@"SNP26", @"SNP26"),
                    new InputOutputColumnPair(@"SNP27", @"SNP27"),
                    new InputOutputColumnPair(@"SNP28", @"SNP28"),
                    new InputOutputColumnPair(@"SNP29", @"SNP29"),
                    new InputOutputColumnPair(@"SNP30", @"SNP30"),
                    new InputOutputColumnPair(@"SNP31", @"SNP31"),
                    new InputOutputColumnPair(@"SNP32", @"SNP32"),
                    new InputOutputColumnPair(@"SNP33", @"SNP33"),
                    new InputOutputColumnPair(@"SNP34", @"SNP34"),
                    new InputOutputColumnPair(@"SNP35", @"SNP35"),
                    new InputOutputColumnPair(@"SNP36", @"SNP36"),
                    new InputOutputColumnPair(@"SNP37", @"SNP37"),
                    new InputOutputColumnPair(@"SNP38", @"SNP38"),
                    new InputOutputColumnPair(@"SNP39", @"SNP39"),
                    new InputOutputColumnPair(@"SNP40", @"SNP40"),
                    new InputOutputColumnPair(@"SNP41", @"SNP41"),
                    new InputOutputColumnPair(@"SNP42", @"SNP42"),
                    new InputOutputColumnPair(@"SNP43", @"SNP43"),
                    new InputOutputColumnPair(@"SNP44", @"SNP44"),
                    new InputOutputColumnPair(@"SNP45", @"SNP45"),
                    new InputOutputColumnPair(@"SNP46", @"SNP46"),
                    new InputOutputColumnPair(@"SNP47", @"SNP47"),
                    new InputOutputColumnPair(@"SNP48", @"SNP48"),
                    new InputOutputColumnPair(@"SNP49", @"SNP49"),
                    new InputOutputColumnPair(@"SNP50", @"SNP50")
                })
                .Append(mlContext.Transforms.Concatenate(@"Features", @"SNP1",
                    @"SNP2", @"SNP3", @"SNP4", @"SNP5", @"SNP6", @"SNP7",
                    @"SNP8",
                    @"SNP9", @"SNP10", @"SNP11", @"SNP12", @"SNP13", @"SNP14",
                    @"SNP15", @"SNP16", @"SNP17", @"SNP18", @"SNP19", @"SNP20",
                    @"SNP21", @"SNP22", @"SNP23", @"SNP24", @"SNP25", @"SNP26",
                    @"SNP27", @"SNP28", @"SNP29", @"SNP30", @"SNP31", @"SNP32",
                    @"SNP33", @"SNP34", @"SNP35", @"SNP36", @"SNP37", @"SNP38",
                    @"SNP39", @"SNP40", @"SNP41", @"SNP42", @"SNP43", @"SNP44",
                    @"SNP45", @"SNP46", @"SNP47", @"SNP48", @"SNP49", @"SNP50"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                    @"y",
                    keyData: lookupIdvMap))
                .Append(trainer);

            var mlModel = pipeline.Fit(trainData);
            Assert.IsNotNull(mlModel);

            var testResults = mlModel.Transform(testData);
            var metrics = mlContext.BinaryClassification
                .Evaluate(testResults);
            var acc = metrics.Accuracy.ToString(CultureInfo.InvariantCulture);
            var f1 = metrics.F1Score.ToString(CultureInfo.InvariantCulture);
            var auc =
                metrics.AreaUnderPrecisionRecallCurve.ToString(CultureInfo
                    .InvariantCulture);
            var aucroc =
                metrics.AreaUnderRocCurve.ToString(CultureInfo
                    .InvariantCulture);
            writer.WriteLine(f1);

            writer.Flush();
            var interpretedModel = GetTrainerModel(trainer);
            if (interpretedModel is not null)
            {
                logWriter.WriteLine(interpretedModel);
            }

            logWriter.WriteLine($"Accuracy: {acc}");
            logWriter.WriteLine($"F1 Score: {f1}");
            logWriter.WriteLine($"AreaUnderPrecisionRecallCurve: {auc}");
            logWriter.WriteLine($"AreaUnderRocCurve: {aucroc}");
            logWriter.Flush();
        }
    }

    private static object? GetTrainerModel(object trainer)
    {
        return trainer.GetType().GetProperty("Model")?.GetValue(trainer);
    }
}