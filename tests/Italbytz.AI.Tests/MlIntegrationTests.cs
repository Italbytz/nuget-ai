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
        const string breastCsv = "radius1,texture1,perimeter1,area1,smoothness1,compactness1,concavity1,concave_points1,symmetry1,fractal_dimension1,radius2,texture2,perimeter2,area2,smoothness2,compactness2,concavity2,concave_points2,symmetry2,fractal_dimension2,radius3,texture3,perimeter3,area3,smoothness3,compactness3,concavity3,concave_points3,symmetry3,fractal_dimension3,Diagnosis\n" +
                                 "12.1,14.5,78.2,460.0,0.090,0.060,0.025,0.020,0.170,0.058,0.28,1.20,2.00,20.0,0.007,0.015,0.012,0.008,0.018,0.002,13.2,16.0,85.0,520.0,0.120,0.150,0.090,0.040,0.250,0.075,B\n" +
                                 "11.8,13.9,76.5,440.0,0.088,0.055,0.020,0.018,0.165,0.057,0.25,1.10,1.80,18.0,0.006,0.014,0.010,0.007,0.017,0.002,12.9,15.1,82.8,495.0,0.118,0.140,0.080,0.035,0.240,0.072,B\n" +
                                 "18.5,21.4,122.0,1090.0,0.104,0.145,0.190,0.100,0.195,0.066,1.05,1.90,7.50,110.0,0.009,0.040,0.050,0.018,0.025,0.004,24.5,28.0,165.0,1850.0,0.145,0.380,0.450,0.210,0.310,0.095,M\n" +
                                 "17.9,20.8,118.4,1020.0,0.101,0.138,0.180,0.094,0.190,0.064,0.98,1.80,7.00,102.0,0.008,0.036,0.046,0.017,0.024,0.004,23.8,27.2,158.0,1720.0,0.141,0.360,0.420,0.195,0.300,0.091,M\n";
        const string carCsv = "buying,maint,doors,persons,lug_boot,safety,class\n" +
                              "vhigh,vhigh,two,two,small,low,unacc\n" +
                              "high,high,three,two,small,med,unacc\n" +
                              "med,med,four,four,med,med,acc\n" +
                              "low,med,four,more,big,med,acc\n" +
                              "med,low,four,more,big,high,good\n" +
                              "low,low,three,more,med,high,good\n" +
                              "low,low,four,more,big,high,vgood\n" +
                              "low,med,fiveormore,more,big,high,vgood\n";
        const string solarCsv = "modified Zurich class,largest spot size,spot distribution,activity,evolution,previous 24 hour flare activity,historically-complex,became complex on this pass,area,area of largest spot,flares\n" +
                                "A,X,X,1,1,0,0,0,10,1,0\n" +
                                "B,R,O,2,1,1,0,0,20,2,1\n" +
                                "C,S,I,3,2,1,1,0,30,3,2\n" +
                                "D,A,C,4,2,2,1,1,40,4,3\n" +
                                "E,H,O,5,3,2,1,1,50,5,4\n" +
                                "F,K,I,6,3,3,1,1,60,6,5\n" +
                                "H,R,C,7,4,3,1,1,70,7,6\n" +
                                "C,H,O,8,4,4,1,1,80,8,8\n";
        const string lensesCsv = "age,spectacle_prescription,astigmatic,class\n" +
                                 "1,1,1,1\n" +
                                 "1,2,1,1\n" +
                                 "2,1,2,2\n" +
                                 "2,2,2,2\n" +
                                 "3,1,1,3\n" +
                                 "3,2,1,3\n";
        const string balanceCsv = "right-distance,right-weight,left-distance,left-weight,class\n" +
                                  "1,1,1,1,B\n" +
                                  "2,2,1,1,R\n" +
                                  "1,1,2,2,L\n";

        var heartPath = Path.Combine(Path.GetTempPath(), $"heart-{Guid.NewGuid():N}.csv");
        var winePath = Path.Combine(Path.GetTempPath(), $"wine-{Guid.NewGuid():N}.csv");
        var breastPath = Path.Combine(Path.GetTempPath(), $"breast-{Guid.NewGuid():N}.csv");
        var carPath = Path.Combine(Path.GetTempPath(), $"car-{Guid.NewGuid():N}.csv");
        var solarPath = Path.Combine(Path.GetTempPath(), $"solar-{Guid.NewGuid():N}.csv");
        var lensesPath = Path.Combine(Path.GetTempPath(), $"lenses-{Guid.NewGuid():N}.csv");
        var balancePath = Path.Combine(Path.GetTempPath(), $"balance-{Guid.NewGuid():N}.csv");
        File.WriteAllText(heartPath, heartCsv);
        File.WriteAllText(winePath, wineCsv);
        File.WriteAllText(breastPath, breastCsv);
        File.WriteAllText(carPath, carCsv);
        File.WriteAllText(solarPath, solarCsv);
        File.WriteAllText(lensesPath, lensesCsv);
        File.WriteAllText(balancePath, balanceCsv);

        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;

            var heartDataset = new HeartDiseaseDataset();
            var heartData = heartDataset.LoadFromTextFile(heartPath);
            var heartTransformed = heartDataset.BuildPreprocessingPipeline(mlContext).Fit(heartData).Transform(heartData);

            var wineDataset = new WineQualityDataset();
            var wineData = wineDataset.LoadFromTextFile(winePath);
            var wineTransformed = wineDataset.BuildPreprocessingPipeline(mlContext).Fit(wineData).Transform(wineData);

            var breastDataset = new BreastCancerWisconsinDiagnosticDataset();
            var breastData = breastDataset.LoadFromTextFile(breastPath);
            var breastTransformed = breastDataset.BuildPreprocessingPipeline(mlContext).Fit(breastData).Transform(breastData);

            var carDataset = new CarEvaluationDataset();
            var carData = carDataset.LoadFromTextFile(carPath);
            var carTransformed = carDataset.BuildPreprocessingPipeline(mlContext).Fit(carData).Transform(carData);

            var solarDataset = new SolarFlareDataset();
            var solarData = solarDataset.LoadFromTextFile(solarPath);
            var solarTransformed = solarDataset.BuildPreprocessingPipeline(mlContext).Fit(solarData).Transform(solarData);

            var lensesDataset = new LensesDataset();
            var lensesData = lensesDataset.LoadFromTextFile(lensesPath);
            var lensesTransformed = lensesDataset.BuildPreprocessingPipeline(mlContext).Fit(lensesData).Transform(lensesData);

            var balanceDataset = new BalanceScaleDataset();
            var balanceData = balanceDataset.LoadFromTextFile(balancePath);
            var balanceTransformed = balanceDataset.BuildPreprocessingPipeline(mlContext).Fit(balanceData).Transform(balanceData);

            Assert.HasCount(14, heartDataset.ColumnProperties);
            Assert.AreEqual("num", heartDataset.LabelColumnName);
            Assert.IsNotNull(heartTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(heartTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(12, wineDataset.ColumnProperties);
            Assert.AreEqual("quality", wineDataset.LabelColumnName);
            Assert.IsNotNull(wineTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(wineTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(31, breastDataset.ColumnProperties);
            Assert.AreEqual("Diagnosis", breastDataset.LabelColumnName);
            Assert.IsNotNull(breastTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(breastTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(7, carDataset.ColumnProperties);
            Assert.AreEqual("class", carDataset.LabelColumnName);
            Assert.IsNotNull(carTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(carTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(11, solarDataset.ColumnProperties);
            Assert.AreEqual("flares", solarDataset.LabelColumnName);
            Assert.IsNotNull(solarTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(solarTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(4, lensesDataset.ColumnProperties);
            Assert.AreEqual("class", lensesDataset.LabelColumnName);
            Assert.IsNotNull(lensesTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(lensesTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));

            Assert.HasCount(5, balanceDataset.ColumnProperties);
            Assert.AreEqual("class", balanceDataset.LabelColumnName);
            Assert.IsNotNull(balanceTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Features));
            Assert.IsNotNull(balanceTransformed.Schema.GetColumnOrNull(DefaultColumnNames.Label));
        }
        finally
        {
            File.Delete(heartPath);
            File.Delete(winePath);
            File.Delete(breastPath);
            File.Delete(carPath);
            File.Delete(solarPath);
            File.Delete(lensesPath);
            File.Delete(balancePath);
        }
    }

    [TestMethod]
    public void Data_registry_exposes_supported_uci_dataset_descriptors()
    {
        Assert.AreEqual("iris", Data.Iris.FilePrefix);
        Assert.AreEqual("heart_disease", Data.HeartDisease.FilePrefix);
        Assert.AreEqual("wine_quality", Data.WineQuality.FilePrefix);
        Assert.AreEqual("breast_cancer_wisconsin_diagnostic", Data.BreastCancerWisconsinDiagnostic.FilePrefix);
        Assert.AreEqual("car_evaluation", Data.CarEvaluation.FilePrefix);
        Assert.AreEqual("solar_flare", Data.SolarFlare.FilePrefix);
        Assert.AreEqual("lenses", Data.Lenses.FilePrefix);
        Assert.AreEqual("balance_scale", Data.BalanceScale.FilePrefix);
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_iris_starter_data()
    {
        const string csv = "sepal length,sepal width,petal length,petal width,class\n" +
                           "5.1,3.5,1.4,0.2,Iris-setosa\n" +
                           "4.9,3.0,1.4,0.2,Iris-setosa\n" +
                           "6.0,2.2,4.0,1.0,Iris-versicolor\n" +
                           "5.9,3.0,4.2,1.5,Iris-versicolor\n" +
                           "6.3,3.3,6.0,2.5,Iris-virginica\n" +
                           "6.5,3.0,5.8,2.2,Iris-virginica\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-iris-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new IrisDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(6, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_balance_scale_starter_data()
    {
        const string csv = "right-distance,right-weight,left-distance,left-weight,class\n" +
                           "1,1,1,1,B\n" +
                           "2,2,1,1,R\n" +
                           "3,1,1,3,B\n" +
                           "1,1,2,2,L\n" +
                           "3,2,1,1,R\n" +
                           "1,1,3,2,L\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-balance-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new BalanceScaleDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(6, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_car_evaluation_starter_data()
    {
        const string csv = "buying,maint,doors,persons,lug_boot,safety,class\n" +
                           "vhigh,vhigh,two,two,small,low,unacc\n" +
                           "high,high,three,two,small,med,unacc\n" +
                           "med,med,four,four,med,med,acc\n" +
                           "low,med,four,more,big,med,acc\n" +
                           "med,low,four,more,big,high,good\n" +
                           "low,low,three,more,med,high,good\n" +
                           "low,low,four,more,big,high,vgood\n" +
                           "low,med,fiveormore,more,big,high,vgood\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-car-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new CarEvaluationDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(8, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_solar_flare_starter_data()
    {
        const string csv = "modified Zurich class,largest spot size,spot distribution,activity,evolution,previous 24 hour flare activity,historically-complex,became complex on this pass,area,area of largest spot,flares\n" +
                           "A,X,X,1,1,0,0,0,10,1,0\n" +
                           "B,R,O,2,1,1,0,0,20,2,1\n" +
                           "C,S,I,3,2,1,1,0,30,3,2\n" +
                           "D,A,C,4,2,2,1,1,40,4,3\n" +
                           "E,H,O,5,3,2,1,1,50,5,4\n" +
                           "F,K,I,6,3,3,1,1,60,6,5\n" +
                           "H,R,C,7,4,3,1,1,70,7,6\n" +
                           "C,H,O,8,4,4,1,1,80,8,8\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-solar-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new SolarFlareDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(8, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_lenses_starter_data()
    {
        const string csv = "age,spectacle_prescription,astigmatic,class\n" +
                           "1,1,1,1\n" +
                           "1,2,1,1\n" +
                           "2,1,2,2\n" +
                           "2,2,2,2\n" +
                           "3,1,1,3\n" +
                           "3,2,1,3\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-lenses-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new LensesDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(6, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_multiclass_trainer_fits_wine_quality_starter_data()
    {
        const string csv = "fixed_acidity,volatile_acidity,citric_acid,residual_sugar,chlorides,free_sulfur_dioxide,total_sulfur_dioxide,density,pH,sulphates,alcohol,quality\n" +
                           "7.4,0.82,0.02,2.0,0.085,12.0,32.0,0.9975,3.48,0.55,9.2,5\n" +
                           "7.8,0.76,0.04,2.2,0.080,14.0,40.0,0.9969,3.42,0.61,9.8,5\n" +
                           "6.8,0.42,0.28,2.1,0.070,18.0,44.0,0.9955,3.32,0.68,10.6,6\n" +
                           "6.5,0.36,0.31,2.0,0.065,20.0,46.0,0.9948,3.28,0.72,11.0,6\n" +
                           "6.1,0.20,0.44,1.8,0.054,28.0,62.0,0.9924,3.18,0.82,12.6,7\n" +
                           "5.9,0.18,0.46,1.7,0.050,30.0,66.0,0.9918,3.15,0.86,12.9,7\n";
        var path = Path.Combine(Path.GetTempPath(), $"logicgp-wine-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new WineQualityDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext,
                new DecisionTreeMulticlassTrainer<MulticlassClassificationOutput>());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var predictions = mlContext.Data.CreateEnumerable<LabeledPredictionRow>(transformed, reuseRowObject: false).ToList();

            Assert.HasCount(6, predictions);
            Assert.AreEqual(predictions.Count, predictions.Count(row => row.Label == row.PredictedLabel));
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_binary_trainer_fits_heart_disease_starter_data()
    {
        const string csv = "age,sex,cp,trestbps,chol,fbs,restecg,thalach,exang,oldpeak,slope,ca,thal,num\n" +
                           "45,0,1,110,190,0,0,172,0,0.0,1,0,2,0\n" +
                           "51,0,2,118,210,0,0,168,0,0.1,1,0,2,0\n" +
                           "39,1,1,120,185,0,0,175,0,0.0,1,0,2,0\n" +
                           "67,1,4,160,286,0,2,108,1,1.5,2,3,3,1\n" +
                           "58,1,4,150,270,1,2,111,1,1.2,2,2,3,1\n" +
                           "62,1,4,140,268,0,2,116,1,2.0,2,2,3,1\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-heart-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new HeartDiseaseDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext, new DecisionTreeBinaryTrainer());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var metrics = mlContext.BinaryClassification.Evaluate(transformed);

            Assert.IsGreaterThanOrEqualTo(0.99, metrics.Accuracy);
            Assert.IsGreaterThanOrEqualTo(0.99, metrics.F1Score);
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Decision_tree_binary_trainer_fits_breast_cancer_wisconsin_diagnostic_starter_data()
    {
        const string csv = "radius1,texture1,perimeter1,area1,smoothness1,compactness1,concavity1,concave_points1,symmetry1,fractal_dimension1,radius2,texture2,perimeter2,area2,smoothness2,compactness2,concavity2,concave_points2,symmetry2,fractal_dimension2,radius3,texture3,perimeter3,area3,smoothness3,compactness3,concavity3,concave_points3,symmetry3,fractal_dimension3,Diagnosis\n" +
                           "12.1,14.5,78.2,460.0,0.090,0.060,0.025,0.020,0.170,0.058,0.28,1.20,2.00,20.0,0.007,0.015,0.012,0.008,0.018,0.002,13.2,16.0,85.0,520.0,0.120,0.150,0.090,0.040,0.250,0.075,B\n" +
                           "11.8,13.9,76.5,440.0,0.088,0.055,0.020,0.018,0.165,0.057,0.25,1.10,1.80,18.0,0.006,0.014,0.010,0.007,0.017,0.002,12.9,15.1,82.8,495.0,0.118,0.140,0.080,0.035,0.240,0.072,B\n" +
                           "12.4,15.0,80.1,470.0,0.091,0.062,0.028,0.021,0.171,0.058,0.29,1.25,2.10,21.0,0.007,0.016,0.013,0.008,0.018,0.002,13.5,16.4,86.4,530.0,0.121,0.152,0.092,0.041,0.252,0.076,B\n" +
                           "18.5,21.4,122.0,1090.0,0.104,0.145,0.190,0.100,0.195,0.066,1.05,1.90,7.50,110.0,0.009,0.040,0.050,0.018,0.025,0.004,24.5,28.0,165.0,1850.0,0.145,0.380,0.450,0.210,0.310,0.095,M\n" +
                           "17.9,20.8,118.4,1020.0,0.101,0.138,0.180,0.094,0.190,0.064,0.98,1.80,7.00,102.0,0.008,0.036,0.046,0.017,0.024,0.004,23.8,27.2,158.0,1720.0,0.141,0.360,0.420,0.195,0.300,0.091,M\n" +
                           "18.9,22.0,124.8,1110.0,0.106,0.150,0.198,0.104,0.198,0.067,1.08,1.95,7.70,114.0,0.009,0.041,0.052,0.019,0.026,0.004,25.1,28.5,168.0,1880.0,0.147,0.390,0.460,0.215,0.315,0.097,M\n";

        var path = Path.Combine(Path.GetTempPath(), $"logicgp-breast-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, csv);

        ThreadSafeMLContext.Seed = 42;
        try
        {
            var mlContext = ThreadSafeMLContext.LocalMLContext;
            var dataset = new BreastCancerWisconsinDiagnosticDataset();
            var data = dataset.LoadFromTextFile(path);
            var pipeline = dataset.BuildPipeline(mlContext, new DecisionTreeBinaryTrainer());

            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            var metrics = mlContext.BinaryClassification.Evaluate(transformed);

            Assert.IsGreaterThanOrEqualTo(0.99, metrics.Accuracy);
            Assert.IsGreaterThanOrEqualTo(0.99, metrics.F1Score);
        }
        finally
        {
            ThreadSafeMLContext.Seed = null;
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

    private sealed class LabeledPredictionRow
    {
        [ColumnName(DefaultColumnNames.Label)] public uint Label { get; set; }
        public uint PredictedLabel { get; set; }
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
