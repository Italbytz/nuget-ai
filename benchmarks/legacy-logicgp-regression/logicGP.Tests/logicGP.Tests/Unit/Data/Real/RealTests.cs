using System.Diagnostics;
using System.Globalization;
using Italbytz.AI;
using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace logicGP.Tests;

public abstract class RealTests
{
    protected int[]
        Seeds =
        [
            42, 7, 13, 23, 3, 666, 777, 9, 17, 21
        ]; //[42, 23, 7, 3, 99, 1, 0, 8, 15, 16];

    protected string? LogFile { get; set; } = null;
    protected StreamWriter? LogWriter { get; set; }
    protected StreamWriter? ResultWriter { get; set; }

    private ITransformer Train<TLabel>(MLContext mlContext,
        IEstimator<ITransformer> generalTrainer, IDataView trainData,
        LookupMap<TLabel>[] lookupData)
    {
        var lookupIdvMap = mlContext.Data.LoadFromEnumerable(lookupData);
        var pipeline = GetPipeline(generalTrainer, lookupIdvMap);
        return pipeline.Fit(trainData);
    }

    public void Benchmark<TLabel>(String name,IEstimator<ITransformer> trainer, IDataView trainData,IDataView testData, LookupMap<TLabel>[] lookupData)
    {
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        var file = Path.Combine(folder, $"{name}_runtime.csv");
        var resultWriter = new StreamWriter(file);
        resultWriter.WriteLine("Trainer,TimeMs");
        var stopwatch = new Stopwatch();
        var generations = 10000;
        var magicNumbers = new int[] { 42,  3, 7, 13, 21, 42, 64, 77, 88, 99, 123 };
            foreach (var magicNumber in magicNumbers)
            {
                ThreadSafeRandomNetCore.Seed = magicNumber;
                ThreadSafeMLContext.Seed = magicNumber;
                stopwatch.Reset();
                stopwatch.Start();
                var model = TestFlRw<TLabel>(trainer, trainData, testData, lookupData);
                stopwatch.Stop();
                var interpretedModel = GetTrainerModel(trainer);
                if (interpretedModel is not null)
                {
                    Console.WriteLine(interpretedModel.ToString());
                }
                Console.WriteLine($"{name}\t{stopwatch.ElapsedMilliseconds}ms");
                /*var metrics = new MLContext().MulticlassClassification
                    .Evaluate(model);
                Console.WriteLine($"MacroAccuracy: {metrics.MacroAccuracy.ToString(CultureInfo.InvariantCulture)}");*/
                resultWriter.WriteLine(
                    $"{name},{stopwatch.ElapsedMilliseconds}");
                resultWriter.Flush();
            
        }
        resultWriter.Dispose();
    }
    
    protected void SaveCvSplit(
        IDataView data, string fileName)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var cvResults = mlContext.Data.CrossValidationSplit(data);
        var foldIndex = 0;

        foreach (var fold in cvResults)
        {
            var trainSet = fold.TrainSet;
            var testSet = fold.TestSet;
            var dataFolder =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            SaveAsCsv(trainSet, Path.Combine(dataFolder,
                $"{fileName}_fold_{foldIndex}_train.csv"));
            SaveAsCsv(testSet, Path.Combine(dataFolder,
                $"{fileName}_fold_{foldIndex}_test.csv"));
            foldIndex++;
        }
    }

    private static void SaveAsCsv(IDataView dataView, string filePath)
    {
        using var dataStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write);
        new MLContext().Data.SaveAsText(dataView, dataStream, ',', schema: false);
    }
    
    protected IDataView TestFlRw<TLabel>(
        IEstimator<ITransformer> generalTrainer,
        IDataView trainData, IDataView testData,
        LookupMap<TLabel>[] lookupData)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var mlModel = Train(mlContext, generalTrainer, trainData, lookupData);
        Assert.IsNotNull(mlModel);
        return mlModel.Transform(testData);
    }

    public void SimulateFlRw<TLabel>(IEstimator<ITransformer> trainer,
        IDataView data, LookupMap<TLabel>[] lookupData, int generations = 10000)
    {
        var logFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(logFolder))
            Directory.CreateDirectory(logFolder);
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        if (LogFile != null)
        {
            var logPath = Path.Combine(logFolder,
                $"{LogFile}_{timeStamp}.log");
            LogWriter = new StreamWriter(logPath);
            var resultPath = Path.Combine(logFolder,
                $"{LogFile}_{timeStamp}.csv");
            ResultWriter = new StreamWriter(resultPath);
        }

        foreach (var mlSeed in Seeds)
        foreach (var randomSeed in Seeds)
        {
            var mlContext = new MLContext(mlSeed);
            ThreadSafeRandomNetCore.Seed = randomSeed;
            var trainTestSplit = mlContext.Data.TrainTestSplit(data, 0.2);

            var trainData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            var mlModel = Train(mlContext, trainer, trainData, lookupData);
            Assert.IsNotNull(mlModel);
            /*var chosenIndividual =
                ((LogicGpTrainerBase<ITransformer>)trainer)
                .ChosenIndividual;*/
            var testResults = mlModel.Transform(testData);
            LogWriter?.WriteLine(
                $"ML seed: {mlSeed}, Random seed: {randomSeed}, Generations: {generations}");
            LogWriter?.WriteLine();
            //LogWriter?.Write(chosenIndividual.ToString());
            // ToDO: lookupData größe 2 -> Binär
            LogWriter?.WriteLine();
            if (lookupData.Length > 2)
            {
                var metrics = new MLContext().MulticlassClassification
                    .Evaluate(testResults);
                LogWriter?.WriteLine(
                    $"MacroAccuracy: {metrics.MacroAccuracy.ToString(CultureInfo.InvariantCulture)}");
                ResultWriter?.WriteLine(
                    metrics.MacroAccuracy.ToString(CultureInfo
                        .InvariantCulture));
            }
            else
            {
                var metrics = new MLContext().BinaryClassification
                    .Evaluate(testResults);
                LogWriter?.WriteLine(
                    $"Accuracy: {metrics.Accuracy.ToString(CultureInfo.InvariantCulture)}");
                LogWriter?.WriteLine(
                    $"AUC: {metrics.AreaUnderRocCurve.ToString(CultureInfo.InvariantCulture)}");
                LogWriter?.WriteLine(
                    $"F1Score: {metrics.F1Score.ToString(CultureInfo.InvariantCulture)}");
                LogWriter?.WriteLine(
                    $"AUPRC: {metrics.AreaUnderPrecisionRecallCurve.ToString(CultureInfo.InvariantCulture)}");
                ResultWriter?.WriteLine(
                    $"{metrics.Accuracy.ToString(CultureInfo.InvariantCulture)},{metrics.AreaUnderRocCurve.ToString(CultureInfo.InvariantCulture)},{metrics.F1Score.ToString(CultureInfo.InvariantCulture)},{metrics.F1Score.ToString(CultureInfo.InvariantCulture)},{metrics.AreaUnderPrecisionRecallCurve.ToString(CultureInfo.InvariantCulture)}"
                );
            }

            LogWriter?.Flush();
            ResultWriter?.Flush();
            /*ResultWriter?.WriteLine(
                    chosenIndividual.LatestKnownFitness[0].ToString(
                        CultureInfo.InvariantCulture));*/
        }

        LogWriter?.Close();
        ResultWriter?.Close();
    }

    private static object? GetTrainerModel(object trainer)
    {
        return trainer.GetType().GetProperty("Model")?.GetValue(trainer);
    }

    protected abstract IEstimator<ITransformer?> GetPipeline(
        IEstimator<ITransformer> trainer, IDataView lookupIdvMap);


    private void CleanUp()
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var projectFiles = Directory.GetFiles(directory, "*.cs");
        foreach (var file in projectFiles) File.Delete(file);
        projectFiles = Directory.GetFiles(directory, "*.csproj");
        foreach (var file in projectFiles) File.Delete(file);
    }


    private void RunAutoMLForConfig(string modelFileName)
    {
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var mlnet = new Process();
        mlnet.StartInfo.FileName = "mlnet";
        mlnet.StartInfo.WorkingDirectory =
            AppDomain.CurrentDomain.BaseDirectory;
        //mlnet.StartInfo.Arguments = $"train --training-config config.mbconfig --log-file-path {timeStamp}.log";
        //mlnet.StartInfo.Arguments = "train --training-config config.mbconfig";
        mlnet.StartInfo.Arguments =
            $"train --training-config {modelFileName}.mbconfig -v q";
        mlnet.Start();
        mlnet.WaitForExit();
    }

    private void PrintAccuracies(Dictionary<string, float> bestMacroaccuracy)
    {
        foreach (var entry in bestMacroaccuracy)
            Console.WriteLine(
                $"{entry.Key}: {entry.Value.ToString(CultureInfo.InvariantCulture)}");

        Console.WriteLine(
            (bestMacroaccuracy["FastTreeOva"] * 100).ToString(CultureInfo
                .InvariantCulture));
        Console.WriteLine(
            (bestMacroaccuracy["FastForestOva"] * 100).ToString(CultureInfo
                .InvariantCulture));
        Console.WriteLine(
            (bestMacroaccuracy["LbfgsMaximumEntropyMulti"] * 100).ToString(
                CultureInfo.InvariantCulture));
        Console.WriteLine(
            (bestMacroaccuracy["SdcaLogisticRegressionOva"] * 100).ToString(
                CultureInfo.InvariantCulture));
        Console.WriteLine(
            (bestMacroaccuracy["SdcaMaximumEntropyMulti"] * 100).ToString(
                CultureInfo.InvariantCulture));
        Console.WriteLine(
            (bestMacroaccuracy["LbfgsLogisticRegressionOva"] * 100).ToString(
                CultureInfo.InvariantCulture));
    }

    private void UpdateAndFilterAccuracies(Dictionary<string, float> accuracies,
        Dictionary<string, float> bestMacroaccuracy)
    {
        foreach (var accuracy in accuracies.Where(accuracy =>
                     !bestMacroaccuracy.ContainsKey(accuracy.Key) ||
                     bestMacroaccuracy[accuracy.Key] < accuracy.Value))
        {
            if (!accuracy.Key.Equals("FastTreeOva") &&
                !accuracy.Key.Equals("LbfgsMaximumEntropyMulti") &&
                !accuracy.Key.Equals("FastForestOva") &&
                !accuracy.Key.Equals("SdcaLogisticRegressionOva") &&
                !accuracy.Key.Equals("LbfgsLogisticRegressionOva") &&
                !accuracy.Key.Equals("SdcaMaximumEntropyMulti")) continue;
            bestMacroaccuracy[accuracy.Key] = accuracy.Value;
        }
    }
}