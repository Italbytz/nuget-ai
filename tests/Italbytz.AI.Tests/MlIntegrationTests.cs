using System.Globalization;
using Italbytz.AI.Learning;
using Italbytz.AI.ML;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Trainers;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.Tests;

[TestClass]
public class MlIntegrationTests
{
    [TestMethod]
    public void Thread_safe_ml_context_is_reused_until_seed_changes()
    {
        ThreadSafeMLContext.Seed = 42;
        var context1 = ThreadSafeMLContext.LocalMLContext;
        var context2 = ThreadSafeMLContext.LocalMLContext;

        Assert.AreSame(context1, context2);

        ThreadSafeMLContext.Seed = 7;
        var context3 = ThreadSafeMLContext.LocalMLContext;

        Assert.AreNotSame(context1, context3);
        ThreadSafeMLContext.Seed = null;
    }

    [TestMethod]
    public void Data_excerpt_can_be_translated_to_learning_dataset()
    {
        var excerpt = new DataExcerpt(
            [
                [0f, 1f],
                [1f, 0f]
            ],
            ["f1", "f2"],
            [1u, 2u]);

        var specification = excerpt.GetDataSetSpecification();
        var dataSet = excerpt.GetDataSet(specification);

        Assert.HasCount(2, dataSet.Examples);
        Assert.AreEqual("1", dataSet.Examples[0].TargetValue());
        Assert.AreEqual("0", dataSet.Examples[1].GetAttributeValueAsString("f2"));
    }

    [TestMethod]
    public void Iris_dataset_loads_csv_and_builds_preprocessing_pipeline()
    {
        const string csv = "sepal length,sepal width,petal length,petal width,class\n" +
                           "5.1,3.5,1.4,0.2,Iris-setosa\n" +
                           "6.0,2.2,4.0,1.0,Iris-versicolor\n" +
                           "6.3,3.3,6.0,2.5,Iris-virginica\n";

        var path = Path.Combine(Path.GetTempPath(), $"iris-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        try
        {
            var dataset = new IrisDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPreprocessingPipeline(ThreadSafeMLContext.LocalMLContext);
            var transformed = pipeline.Fit(data).Transform(data);

            Assert.HasCount(5, dataset.ColumnProperties);
            Assert.IsNotNull(transformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(transformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_binary_trainer_fits_restaurant_data()
    {
        const string csv = "alternate,bar,fri/sat,hungry,patrons,price,raining,reservation,type,wait_estimate,will_wait\n" +
                           "1,0,0,1,1,2,0,1,0,0,1\n" +
                           "0,1,0,0,0,0,1,0,1,1,0\n" +
                           "1,1,1,1,2,2,0,1,2,2,1\n" +
                           "0,0,1,0,0,1,1,0,3,3,0\n";

        var path = Path.Combine(Path.GetTempPath(), $"restaurant-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var data = mlContext.Data.LoadFromTextFile<RestaurantModelInput>(path, ',', true);
            LookupMap<uint>[] lookupData = [new(0u), new(1u)];
            var lookupDataView = mlContext.Data.LoadFromEnumerable(lookupData);

            var trainer = new DecisionTreeBinaryTrainer();
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair("alternate"),
                    new InputOutputColumnPair("bar"),
                    new InputOutputColumnPair("fri/sat"),
                    new InputOutputColumnPair("hungry"),
                    new InputOutputColumnPair("patrons"),
                    new InputOutputColumnPair("price"),
                    new InputOutputColumnPair("raining"),
                    new InputOutputColumnPair("reservation"),
                    new InputOutputColumnPair("type"),
                    new InputOutputColumnPair("wait_estimate")
                })
                .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                    "alternate", "bar", "fri/sat", "hungry", "patrons", "price", "raining", "reservation", "type", "wait_estimate"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(DefaultColumnNames.Label, "will_wait", keyData: lookupDataView))
                .Append(trainer);

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var metrics = mlContext.BinaryClassification.Evaluate(transformed);

            Assert.AreEqual(1.0, metrics.Accuracy, 0.0001);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class RestaurantModelInput
    {
        [LoadColumn(0)] [ColumnName("alternate")] public float Alternate { get; set; }
        [LoadColumn(1)] [ColumnName("bar")] public float Bar { get; set; }
        [LoadColumn(2)] [ColumnName("fri/sat")] public float FriSat { get; set; }
        [LoadColumn(3)] [ColumnName("hungry")] public float Hungry { get; set; }
        [LoadColumn(4)] [ColumnName("patrons")] public float Patrons { get; set; }
        [LoadColumn(5)] [ColumnName("price")] public float Price { get; set; }
        [LoadColumn(6)] [ColumnName("raining")] public float Raining { get; set; }
        [LoadColumn(7)] [ColumnName("reservation")] public float Reservation { get; set; }
        [LoadColumn(8)] [ColumnName("type")] public float Type { get; set; }
        [LoadColumn(9)] [ColumnName("wait_estimate")] public float WaitEstimate { get; set; }
        [LoadColumn(10)] [ColumnName("will_wait")] public uint WillWait { get; set; }
    }
}
