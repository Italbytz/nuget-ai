using System.Globalization;
using Italbytz.AI.Learning;
using Italbytz.AI.ML;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.Trainers;
using Italbytz.AI.ML.UciDatasets;
using Microsoft.ML;
using Microsoft.ML.Data;
using InputOutputColumnPair = Microsoft.ML.InputOutputColumnPair;

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
    public void Feature_descriptors_capture_value_ranges()
    {
        var categorical = new CategoricalFeature
        {
            PropertyName = "Color",
            ColumnName = "color",
            ColumnIndex = 2,
            ValueRange = ["red", "green", "blue"]
        };
        var numerical = new NumericalFeature
        {
            PropertyName = "Weight",
            ColumnName = "weight",
            ColumnIndex = 3,
            ValueRange = [0.5f, 1.5f, 2.5f]
        };

        CollectionAssert.AreEqual(new[] { "red", "green", "blue" }, categorical.ValueRange);
        CollectionAssert.AreEqual(new[] { 0.5f, 1.5f, 2.5f }, numerical.ValueRange);
        Assert.AreEqual("weight", numerical.ColumnName);
    }

    [TestMethod]
    public void Training_configuration_serializes_without_null_values()
    {
        var config = new TrainingConfiguration
        {
            Scenario = ScenarioType.Classification,
            DataSource = new TabularFileDataSourceV3
            {
                FilePath = "data.csv",
                Delimiter = ",",
                DecimalMarker = '.',
                HasHeader = true,
                AllowQuoting = false,
                EscapeCharacter = '\\',
                ReadMultiLines = false,
                ColumnProperties =
                [
                    new ColumnPropertiesV5
                    {
                        Type = "Column",
                        ColumnName = "label",
                        ColumnDataFormat = ColumnDataKind.Boolean,
                        ColumnPurpose = ColumnPurposeType.AnswerIndex,
                        IsCategorical = false
                    }
                ]
            },
            Environment = new LocalEnvironmentV1
            {
                Type = "LocalCPU",
                EnvironmentType = EnvironmentType.LocalCPU
            },
            TrainingOption = new ClassificationTrainingOptionV2
            {
                LabelColumn = "Label",
                AvailableTrainers = ["FastTree", "SdcaMaximumEntropy"],
                TrainingTime = 10,
                ValidationOption = new TrainValidationSplitOptionV0
                {
                    SplitRatio = 0.1f
                }
            }
        };

        var json = config.SerializeToJson();

        StringAssert.Contains(json, "FastTree");
        StringAssert.Contains(json, "\"Scenario\":\"Classification\"");
        Assert.DoesNotContain(json, "null");
    }

    [TestMethod]
    public void Explainer_generates_permutation_importance_and_ceteris_paribus_tables()
    {
        const string csv = "sepal length,sepal width,petal length,petal width,class\n" +
                           "5.1,3.5,1.4,0.2,Iris-setosa\n" +
                           "4.9,3.0,1.4,0.2,Iris-setosa\n" +
                           "6.0,2.2,4.0,1.0,Iris-versicolor\n" +
                           "5.9,3.0,4.2,1.5,Iris-versicolor\n" +
                           "6.3,3.3,6.0,2.5,Iris-virginica\n" +
                           "6.5,3.0,5.8,2.2,Iris-virginica\n";

        var path = Path.Combine(Path.GetTempPath(), $"iris-explainer-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var data = mlContext.Data.LoadFromTextFile<IrisLikeModelInput>(path, ',', true);
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair("sepal length"),
                    new InputOutputColumnPair("sepal width"),
                    new InputOutputColumnPair("petal length"),
                    new InputOutputColumnPair("petal width")
                })
                .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                    "sepal length", "sepal width", "petal length", "petal width"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(DefaultColumnNames.Label, "class"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: DefaultColumnNames.Label,
                    featureColumnName: DefaultColumnNames.Features));

            var model = pipeline.Fit(data);
            var explainer = new Explainer(model, data, ScenarioType.Classification, "class");

            var pfi = explainer.GetPermutationFeatureImportanceTable(Metric.MacroAccuracy);
            var ceterisParibus = explainer.GetCeterisParibusTable<IrisLikeModelInput, MulticlassClassificationOutput>(0, 5);

            StringAssert.Contains(pfi, "Feature, Importance");
            Assert.IsGreaterThan(1, pfi.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
            StringAssert.Contains(ceterisParibus, "Feature,Score,Class");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Interpreter_externalizes_model_parameters_from_multiclass_pipeline()
    {
        const string csv = "sepal length,sepal width,petal length,petal width,class\n" +
                           "5.1,3.5,1.4,0.2,Iris-setosa\n" +
                           "4.9,3.0,1.4,0.2,Iris-setosa\n" +
                           "6.0,2.2,4.0,1.0,Iris-versicolor\n" +
                           "5.9,3.0,4.2,1.5,Iris-versicolor\n" +
                           "6.3,3.3,6.0,2.5,Iris-virginica\n" +
                           "6.5,3.0,5.8,2.2,Iris-virginica\n";

        var path = Path.Combine(Path.GetTempPath(), $"iris-interpreter-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var data = mlContext.Data.LoadFromTextFile<IrisLikeModelInput>(path, ',', true);
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair("sepal length"),
                    new InputOutputColumnPair("sepal width"),
                    new InputOutputColumnPair("petal length"),
                    new InputOutputColumnPair("petal width")
                })
                .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                    "sepal length", "sepal width", "petal length", "petal width"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(DefaultColumnNames.Label, "class"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: DefaultColumnNames.Label,
                    featureColumnName: DefaultColumnNames.Features));

            var model = pipeline.Fit(data);
            var interpreter = new Interpreter(model);
            var parameters = interpreter.ExternalizedModelParameters;

            Assert.IsNotNull(parameters);
            StringAssert.Contains(parameters.GetType().Name, "ModelParameters");
        }
        finally
        {
            File.Delete(path);
        }
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
    public void Additional_uci_datasets_load_and_build_preprocessing_pipelines()
    {
        const string heartCsv = "age,sex,cp,trestbps,chol,fbs,restecg,thalach,exang,oldpeak,slope,ca,thal,num\n" +
                                "63,1,3,145,233,1,0,150,0,2.3,0,0,1,1\n" +
                                "37,1,2,130,250,0,1,187,0,3.5,0,0,2,0\n" +
                                "41,0,1,130,204,0,0,172,0,1.4,2,0,2,0\n";
        const string wineCsv = "fixed_acidity,volatile_acidity,citric_acid,residual_sugar,chlorides,free_sulfur_dioxide,total_sulfur_dioxide,density,pH,sulphates,alcohol,quality\n" +
                               "7.4,0.70,0.00,1.9,0.076,11.0,34.0,0.9978,3.51,0.56,9.4,5\n" +
                               "7.8,0.88,0.00,2.6,0.098,25.0,67.0,0.9968,3.20,0.68,9.8,5\n" +
                               "7.3,0.65,0.00,1.2,0.065,15.0,21.0,0.9946,3.39,0.47,10.0,7\n";

        var heartPath = Path.Combine(Path.GetTempPath(), $"heart-{Guid.NewGuid():N}.csv");
        var winePath = Path.Combine(Path.GetTempPath(), $"wine-{Guid.NewGuid():N}.csv");
        File.WriteAllText(heartPath, heartCsv);
        File.WriteAllText(winePath, wineCsv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;

            var heartDataset = new HeartDiseaseDataset();
            var heartData = heartDataset.LoadFromTextFile(heartPath);
            var heartTransformed = heartDataset.BuildPreprocessingPipeline(mlContext).Fit(heartData).Transform(heartData);

            var wineDataset = new WineQualityDataset();
            var wineData = wineDataset.LoadFromTextFile(winePath);
            var wineTransformed = wineDataset.BuildPreprocessingPipeline(mlContext).Fit(wineData).Transform(wineData);

            Assert.HasCount(14, heartDataset.ColumnProperties);
            Assert.AreEqual("num", heartDataset.LabelColumnName);
            Assert.IsNotNull(heartTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(heartTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(12, wineDataset.ColumnProperties);
            Assert.AreEqual("quality", wineDataset.LabelColumnName);
            Assert.IsNotNull(wineTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(wineTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));
        }
        finally
        {
            File.Delete(heartPath);
            File.Delete(winePath);
        }
    }

    [TestMethod]
    public void Data_registry_exposes_supported_uci_dataset_descriptors()
    {
        Assert.AreEqual("iris", Data.Iris.FilePrefix);
        Assert.AreEqual("heart_disease", Data.HeartDisease.FilePrefix);
        Assert.AreEqual("wine_quality", Data.WineQuality.FilePrefix);
        Assert.AreEqual("breast_cancer_wisconsin_diagnostic", Data.BreastCancerWisconsinDiagnostic.FilePrefix);
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

    [TestMethod]
    public void Least_squares_trainer_fits_simple_regression_data()
    {
        const string csv = "x1,x2,y\n" +
                           "1,1,9\n" +
                           "2,1,12\n" +
                           "1,2,13\n" +
                           "3,2,19\n";

        var path = Path.Combine(Path.GetTempPath(), $"regression-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var data = mlContext.Data.LoadFromTextFile<RegressionModelInput>(path, ',', true);
            var trainer = new LeastSquaresTrainer();
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair("x1"),
                    new InputOutputColumnPair("x2"),
                    new InputOutputColumnPair(DefaultColumnNames.Label, "y")
                })
                .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features, "x1", "x2"))
                .Append(trainer);

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var metrics = mlContext.Regression.Evaluate(transformed);

            Assert.AreEqual(0.0, metrics.MeanSquaredError, 0.0001);
            Assert.AreEqual(1.0, metrics.RSquared, 0.0001);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class IrisLikeModelInput
    {
        [LoadColumn(0)] [ColumnName("sepal length")] public float SepalLength { get; set; }
        [LoadColumn(1)] [ColumnName("sepal width")] public float SepalWidth { get; set; }
        [LoadColumn(2)] [ColumnName("petal length")] public float PetalLength { get; set; }
        [LoadColumn(3)] [ColumnName("petal width")] public float PetalWidth { get; set; }
        [LoadColumn(4)] [ColumnName("class")] public string Class { get; set; } = string.Empty;
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

    private sealed class RegressionModelInput
    {
        [LoadColumn(0)] [ColumnName("x1")] public float X1 { get; set; }
        [LoadColumn(1)] [ColumnName("x2")] public float X2 { get; set; }
        [LoadColumn(2)] [ColumnName("y")] public float Y { get; set; }
    }
}
