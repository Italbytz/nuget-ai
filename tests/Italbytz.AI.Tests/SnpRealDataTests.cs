using System;
using System.IO;
using System.Linq;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.LogicGp;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.Tests;

internal class ScrimeSNPModelInput
{
    [LoadColumn(0)]
    [ColumnName("SNP1")]
    public float SNP1 { get; set; }

    [LoadColumn(1)]
    [ColumnName("SNP2")]
    public float SNP2 { get; set; }

    [LoadColumn(2)]
    [ColumnName("SNP3")]
    public float SNP3 { get; set; }

    [LoadColumn(3)]
    [ColumnName("SNP4")]
    public float SNP4 { get; set; }

    [LoadColumn(4)]
    [ColumnName("SNP5")]
    public float SNP5 { get; set; }

    [LoadColumn(5)]
    [ColumnName("SNP6")]
    public float SNP6 { get; set; }

    [LoadColumn(6)]
    [ColumnName("SNP7")]
    public float SNP7 { get; set; }

    [LoadColumn(7)]
    [ColumnName("SNP8")]
    public float SNP8 { get; set; }

    [LoadColumn(8)]
    [ColumnName("SNP9")]
    public float SNP9 { get; set; }

    [LoadColumn(9)]
    [ColumnName("SNP10")]
    public float SNP10 { get; set; }

    [LoadColumn(10)]
    [ColumnName("SNP11")]
    public float SNP11 { get; set; }

    [LoadColumn(11)]
    [ColumnName("SNP12")]
    public float SNP12 { get; set; }

    [LoadColumn(12)]
    [ColumnName("SNP13")]
    public float SNP13 { get; set; }

    [LoadColumn(13)]
    [ColumnName("SNP14")]
    public float SNP14 { get; set; }

    [LoadColumn(14)]
    [ColumnName("SNP15")]
    public float SNP15 { get; set; }

    [LoadColumn(15)]
    [ColumnName("SNP16")]
    public float SNP16 { get; set; }

    [LoadColumn(16)]
    [ColumnName("SNP17")]
    public float SNP17 { get; set; }

    [LoadColumn(17)]
    [ColumnName("SNP18")]
    public float SNP18 { get; set; }

    [LoadColumn(18)]
    [ColumnName("SNP19")]
    public float SNP19 { get; set; }

    [LoadColumn(19)]
    [ColumnName("SNP20")]
    public float SNP20 { get; set; }

    [LoadColumn(20)]
    [ColumnName("SNP21")]
    public float SNP21 { get; set; }

    [LoadColumn(21)]
    [ColumnName("SNP22")]
    public float SNP22 { get; set; }

    [LoadColumn(22)]
    [ColumnName("SNP23")]
    public float SNP23 { get; set; }

    [LoadColumn(23)]
    [ColumnName("SNP24")]
    public float SNP24 { get; set; }

    [LoadColumn(24)]
    [ColumnName("SNP25")]
    public float SNP25 { get; set; }

    [LoadColumn(25)]
    [ColumnName("SNP26")]
    public float SNP26 { get; set; }

    [LoadColumn(26)]
    [ColumnName("SNP27")]
    public float SNP27 { get; set; }

    [LoadColumn(27)]
    [ColumnName("SNP28")]
    public float SNP28 { get; set; }

    [LoadColumn(28)]
    [ColumnName("SNP29")]
    public float SNP29 { get; set; }

    [LoadColumn(29)]
    [ColumnName("SNP30")]
    public float SNP30 { get; set; }

    [LoadColumn(30)]
    [ColumnName("SNP31")]
    public float SNP31 { get; set; }

    [LoadColumn(31)]
    [ColumnName("SNP32")]
    public float SNP32 { get; set; }

    [LoadColumn(32)]
    [ColumnName("SNP33")]
    public float SNP33 { get; set; }

    [LoadColumn(33)]
    [ColumnName("SNP34")]
    public float SNP34 { get; set; }

    [LoadColumn(34)]
    [ColumnName("SNP35")]
    public float SNP35 { get; set; }

    [LoadColumn(35)]
    [ColumnName("SNP36")]
    public float SNP36 { get; set; }

    [LoadColumn(36)]
    [ColumnName("SNP37")]
    public float SNP37 { get; set; }

    [LoadColumn(37)]
    [ColumnName("SNP38")]
    public float SNP38 { get; set; }

    [LoadColumn(38)]
    [ColumnName("SNP39")]
    public float SNP39 { get; set; }

    [LoadColumn(39)]
    [ColumnName("SNP40")]
    public float SNP40 { get; set; }

    [LoadColumn(40)]
    [ColumnName("SNP41")]
    public float SNP41 { get; set; }

    [LoadColumn(41)]
    [ColumnName("SNP42")]
    public float SNP42 { get; set; }

    [LoadColumn(42)]
    [ColumnName("SNP43")]
    public float SNP43 { get; set; }

    [LoadColumn(43)]
    [ColumnName("SNP44")]
    public float SNP44 { get; set; }

    [LoadColumn(44)]
    [ColumnName("SNP45")]
    public float SNP45 { get; set; }

    [LoadColumn(45)]
    [ColumnName("SNP46")]
    public float SNP46 { get; set; }

    [LoadColumn(46)]
    [ColumnName("SNP47")]
    public float SNP47 { get; set; }

    [LoadColumn(47)]
    [ColumnName("SNP48")]
    public float SNP48 { get; set; }

    [LoadColumn(48)]
    [ColumnName("SNP49")]
    public float SNP49 { get; set; }

    [LoadColumn(49)]
    [ColumnName("SNP50")]
    public float SNP50 { get; set; }

    [LoadColumn(50)]
    [ColumnName("y")]
    public uint Y { get; set; }
}

[TestClass]
public sealed class SnpRealDataTests
{
    [TestMethod]
    [Timeout(1800000)]
    public void Logicgp_flcw_macro_multiclass_trainer_fits_scrime_snp_data()
    {
        var scrimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "Data/SNP/scrime.csv");

        // Rename x.SNP → SNP in header so column names are valid C# identifiers
        var lines = File.ReadAllLines(scrimePath);
        var header = lines[0].Replace("\"x.SNP", "\"SNP");
        var tempPath = Path.Combine(Path.GetTempPath(),
            $"scrime-{Guid.NewGuid():N}.csv");
        try
        {
            File.WriteAllLines(tempPath,
                new[] { header }.Concat(lines.Skip(1)).ToArray());

            var mlContext = ThreadSafeMLContext.LocalMLContext;

            var lookupData = new[]
            {
                new LookupMap<uint>(0),
                new LookupMap<uint>(1),
                new LookupMap<uint>(2)
            };
            var lookupIdvMap =
                mlContext.Data.LoadFromEnumerable(lookupData);

            var allData =
                mlContext.Data.LoadFromTextFile<ScrimeSNPModelInput>(
                    tempPath, ',', true);

            var split = mlContext.Data.TrainTestSplit(allData, testFraction: 0.2,
                seed: 42);
            var trainData = split.TrainSet;
            var testData = split.TestSet;

            var snpCols = Enumerable.Range(1, 50)
                .Select(i => $"SNP{i}")
                .ToArray();

            var replaceMissingPairs = snpCols
                .Select(c => new InputOutputColumnPair(c, c))
                .ToArray();

            var trainer =
                new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
                    2000);

            var pipeline = mlContext.Transforms
                .ReplaceMissingValues(replaceMissingPairs)
                .Append(mlContext.Transforms.Concatenate("Features", snpCols))
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label",
                    "y", keyData: lookupIdvMap))
                .Append(trainer);

            var model = pipeline.Fit(trainData);
            var testResults = model.Transform(testData);
            var metrics =
                mlContext.MulticlassClassification.Evaluate(testResults);

            Console.WriteLine(
                $"MacroAccuracy={metrics.MacroAccuracy:F4}, MicroAccuracy={metrics.MicroAccuracy:F4}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        Assert.IsTrue(true);
    }
}
