# Getting started with ML helpers

The consolidated `Italbytz.AI.ML.Core` and `Italbytz.AI.ML` packages keep the older ML helper ideas, but expose them under the new `Italbytz.AI.*` naming scheme.

## Main building blocks

- `ThreadSafeMLContext` gives you a reusable ML.NET context.
- `LeastSquaresTrainer` provides a compact regression-oriented trainer.
- `Explainer` helps with permutation feature importance and ceteris-paribus tables.
- `Interpreter` lets you inspect and externalize model parameters.

## Simple regression example

```csharp
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Trainers;

var mlContext = ThreadSafeMLContext.LocalMLContext;
var data = mlContext.Data.LoadFromTextFile<RegressionModelInput>(
    path: "regression.csv",
    separatorChar: ',',
    hasHeader: true);

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
```

> Use your own input model type (for example `RegressionModelInput`) to describe the CSV schema you want to load.

## Explainer and Interpreter example

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using Italbytz.AI.ML;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;

var mlContext = ThreadSafeMLContext.LocalMLContext;
var data = mlContext.Data.LoadFromTextFile<IrisLikeModelInput>("iris.csv", ',', true);

var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
    {
        new InputOutputColumnPair("sepal length"),
        new InputOutputColumnPair("sepal width"),
        new InputOutputColumnPair("petal length"),
        new InputOutputColumnPair("petal width")
    })
    .Append(mlContext.Transforms.Concatenate(
        DefaultColumnNames.Features,
        "sepal length", "sepal width", "petal length", "petal width"))
    .Append(mlContext.Transforms.Conversion.MapValueToKey(DefaultColumnNames.Label, "class"))
    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
        labelColumnName: DefaultColumnNames.Label,
        featureColumnName: DefaultColumnNames.Features));

var model = pipeline.Fit(data);

var explainer = new Explainer(model, data, ScenarioType.Classification, "class");
var pfiTable = explainer.GetPermutationFeatureImportanceTable(Metric.MacroAccuracy);

var interpreter = new Interpreter(model);
var parameters = interpreter.ExternalizedModelParameters;
```

## Dataset helpers

For quick experiments, `Italbytz.AI.ML.UciDatasets` already includes curated dataset descriptors such as:

- `IrisDataset`
- `HeartDiseaseDataset`
- `WineQualityDataset`
- `BreastCancerWisconsinDiagnosticDataset`

These descriptors are useful when you want a repeatable, well-known sample dataset instead of wiring CSV metadata manually from scratch.