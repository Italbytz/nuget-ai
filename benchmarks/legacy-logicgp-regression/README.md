# Legacy benchmark note

This folder is the in-repo replacement for the removed standalone consumer `csharp-mstest-logicgp`.

It is kept as a legacy benchmark snapshot inside `nuget-ai` because the larger dataset-driven logicGP material is heavier than the compact regression tests that now live in `nuget-ai/tests/Italbytz.AI.Tests/MlIntegrationTests.cs`. Use this folder for historical comparison runs and dataset-driven benchmark experiments, not as part of the default lightweight test set.

The former source repository for `Italbytz.Adapters.Algorithms.AI` now lives in the workspace's deprecated package area and should be read as historical context for this benchmark snapshot.

# Introduction

logicGP is a generalization of the [Genetic Programming for Association Studies (GPAS)](https://doi.org/10.1093/bioinformatics/btm522) algorithm. It is currently unpublished, first results can be seen on published [slides](https://isd-nunkesser.github.io/slides/ISDBlackHighlyInterpretablePredictionModels.html#/logicgp).
It is intended to be used for all classification tasks. Currently, it is restricted to categorical features. 

The implementation mainly uses the following NuGet packages:

- [Italbytz.Ports.Algorithms.AI](https://www.nuget.org/packages/Italbytz.Ports.Algorithms.AI) (Source: [nuget-ports-algorithms-ai](https://github.com/Italbytz/nuget-ports-algorithms-ai))
- [Italbytz.Adapters.Algorithms.AI](https://www.nuget.org/packages/Italbytz.Adapters.Algorithms.AI) (Legacy source: [nuget-adapters-algorithms-ai](https://github.com/Italbytz/nuget-adapters-algorithms-ai))

The algorithm is intended to be compatible with Microsoft's [ML.NET](https://dotnet.microsoft.com/en-us/apps/ai/ml-dotnet).

# Getting started

The project features unit tests as a starting point. The most important unit tests show the application of the algorithm to example data sets.

## Overview of Unit Tests for Data Sets

### Simulated data sets

|Data set| Code|
| ------ | :---|
|Simulation from Section 3.3 of [Detecting high-order interactions of single nucleotide polymorphisms using genetic programming](https://doi.org/10.1093/bioinformatics/btm522)|[SNPSimulationTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Simulated/SNPSimulationTests.cs)|
|Simulation from Equation (2) of [Evaluation of tree‑based statistical learning methods for constructing genetic risk scores](https://doi.org/10.1186/s12859-022-04634-w)|[SNPSimulationTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Simulated/SNPSimulationTests.cs)|
|Simulation from Equation (3) of [Evaluation of tree‑based statistical learning methods for constructing genetic risk scores](https://doi.org/10.1186/s12859-022-04634-w)|[SNPSimulationTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Simulated/SNPSimulationTests.cs)|

### Real data sets

Chosen data sets from the [UC Irvine Machine Learning Repository](https://archive.ics.uci.edu)

|Data set| Code|
| ------ | :---|
|[Iris](https://archive.ics.uci.edu/dataset/53/iris)|[IrisTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/IrisTests.cs)|
|[Heart Disease](https://archive.ics.uci.edu/dataset/45/heart+disease)|[HeartDiseaseTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/HeartDiseaseTests.cs)|
|[Wine Quality](https://archive.ics.uci.edu/dataset/186/wine+quality)|[WineQualityTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/WineQualityTests.cs)|
|[Breast Cancer Wisconsin (Diagnostic)](https://archive.ics.uci.edu/dataset/17/breast+cancer+wisconsin+diagnostic)|[BreastCancerWisconsinDiagnosticTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/BreastCancerWisconsinDiagnosticTests.cs)
|[National Poll on Healthy Aging (NPHA)](https://archive.ics.uci.edu/dataset/936/national+poll+on+healthy+aging+(npha))|[NationalPollTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/NationalPollTests.cs)|
|[Car Evaluation](https://archive.ics.uci.edu/dataset/19/car+evaluation)|[CarEvaluationTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/CarEvaluationTests.cs)|
|[Balance Scale](https://archive.ics.uci.edu/dataset/12/balance+scale)|[BalanceScaleTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/BalanceScaleTests.cs)|
|[Solar Flare](https://archive.ics.uci.edu/dataset/89/solar+flare)|[SolarflareTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/SolarflareTests.cs)|
|[Lenses](https://archive.ics.uci.edu/dataset/58/lenses)|[LensesTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/LensesTests.cs)|

#### Iris

Example Model found by logicGP-FLCW-Macro on the IRIS data set with $100\%$ MacroAccuracy. The values correspond to a binning of the continuous values to four bins {0,0.34,0.67,1}.

| $w_0$ | $w_1$ | $w_2$ | Condition                                   |
| ----- | ----- | ----- | ------------------------------------------- |
|  1.00 |  0.00 |  0.00 | None below fulfilled                        |
|  0.00 |  2.31 |  1.39 | (petal width ∈ {0.67}) |
|  0.00 |  5.56 |  0.00 | (sepal width ∈ {0,0.34})(petal width ∈ {0.34}) |
|  0.00 |  0.00 |  7.00 | (petal length ∈ {1}) |
|  0.00 |  0.00 |  7.00 | (petal width ∈ {1}) |

#### Heart Disease

Example Model found by logicGP-FLCW-Macro on the IRIS data set with $49.29\%$ MacroAccuracy on the test data. The values correspond to a binning of the continuous values to four bins {0,0.34,0.67,1}.

| $w_0$ | $w_1$ | $w_2$ | $w_3$ | $w_4$ | Condition                                   |
| ----- | ----- | ----- | ----- | ----- | ------------------------------------------- |
|  0.52 |  0.17 |  0.14 |  0.11 |  0.06 | None below fulfilled                        |
|  0.46 |  2.32 |  1.85 |  1.47 |  1.18 | (ca ∈ {0.34}) |
|  3.18 |  1.31 |  0.34 |  0.41 |  0.20 | (ca ∈ {0,0.34}) |
|  0.36 |  1.15 |  2.14 |  2.44 |  3.56 | (thalach ∈ {0,0.67})(ca ∈ {0.34,1}) |
|  0.82 |  1.17 |  2.99 |  0.65 |  1.68 | (age ∈ {0,0.67,1}) |
|  1.71 |  0.56 |  0.46 |  2.01 |  0.38 | (age ∈ {0,0.34,1}) |

#### National Poll on Healthy Aging (NPHA)

Example Model found by logicGP-FLCW-Macro on the NPHA data set with $47.33\%$ MacroAccuracy.

| $w_{0-1}$  | $w_{2-3}$  | $w_{4+}$  | Condition                                                                                     |
|--------|--------|-------|----------------------------------------------------------------------------------------------|
| $1.86$ | $0.84$ | $0.86$ | Employment $\notin$ {Refused,Retired}                                        |
| $0.36$ | $0.86$ | $1.63$ | Sleep Medication $\in$ {Refused,Use regularly}                              |
| $1.49$ | $1.02$ | $0.70$ | Race $\in$ {Hispanic}                                                              |
| $1.47$ | $1.12$ | $0.54$ | Dental Health $\in$ {Excellent,Poor}                                        |
| $1.41$ | $0.95$ | $0.89$ | Dental Health $\notin$ {Excellent,Very Good} $\wedge$ Physical Health $\in$ {Very Good,Good,Poor} |
| $0.81$ | $0.89$ | $1.39$ | Physical Health $\notin$ {Refused,Very Good}                                |
| $1.24$ | $1.01$ | $0.87$ | Mental Health $\in$ {Excellent}                                                   |
| $1.09$ | $0.97$ | $1.00$ | Physical Health $\notin$ {Very Good,Poor}                                   |
| $0.91$ | $1.02$ | $1.02$ | Dental Health $\notin$ {Excellent,Good} $\wedge$ Mental Health $\in$ {Very Good,Good} |
| 0.0 | 1.0  | 0.0  | none of the above

#### Lenses

Example model found by logicGP-FLCW-Macro with $93.33\%$ MacroAccuracy.

| $w_{Hard}$ | $w_{Soft}$ | $w_{None}$  | Condition                                                      |
|------|------|-------|---------------------------------------------------------------------------|
| 0.2  | 5.0  | 1.11  | age ∈ {young} ∧ spectacle prescription ∈ {myope}                         |
| 0.0  | 0.0  | 3.67  | astigmatic ∈ {no}                                                        |
| 0.0  | 3.2  | 0.96  | age ∈ {pre-presbyopic} ∧ spectacle prescription ∈ {myope}               |
| 0.67 | 0.0  | 0.33  | none of the above

# Analyzing Data Sets

We have tried to integrate with Microsoft's [ML.NET](https://dotnet.microsoft.com/en-us/apps/ai/ml-dotnet) as closely as possible. However, ML.NET has many internal APIs and is not always easy to integrate with, so some manual work is needed. 

A good starting point is to first use Microsoft's [AutoML](https://learn.microsoft.com/en-us/dotnet/machine-learning/reference/ml-net-cli-reference) or [Model Builder](https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/load-data-model-builder) to generate an ML.NET input model class and pipeline for your data set. 

## Remarks on data preparation

ML.NET's trainers typically operate on a two-dimensional feature table of ```float``` values and a label column with ```uint``` values describing the classes. 

Data gets prepared for training with [data transformations](https://github.com/dotnet/docs/blob/main/docs/machine-learning/resources/transforms.md). Typically, the following transformations are suggested:

- ```float``` or ```int``` values in a feature are taken as ```float``` to preserve possible ordinal values
- ```string``` values in a feature are one hot encoded
- ```string``` values in the label get a key-value-mapping to ```uint```

Unfortunately, logicGP cannot operate on one hot encoded values presently. The direct use of the key-value-mapping is also not possible due to ML.NET hiding a lot of internal APIs. 

The alternatives (examples may be found in the unit tests) are manual mappings with given key-value-mappings. 

## Iris Example

Let us consider the popular Iris data set for a step-by-step example. The steps are: 

1. Transform your data to ```CSV```
2. Use [AutoML](https://learn.microsoft.com/en-us/dotnet/machine-learning/reference/ml-net-cli-reference) or [Model Builder](https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/load-data-model-builder) to generate an ML.NET project.
3. Write code for logicGP with the help of the generated input model and pipeline. 

### ```CSV``` Data

The [UC Irvine Machine Learning Repository](https://archive.ics.uci.edu) offers a Python package called ```ucimlrepo``` to download data sets in a standardized ```CSV``` format.

```python
from ucimlrepo import fetch_ucirepo 
import ssl
import pandas as pd

ssl._create_default_https_context = ssl._create_unverified_context
  
# fetch dataset 
iris = fetch_ucirepo(id=53) 
  
# data (as pandas dataframes) 
X = iris.data.features 
y = iris.data.targets 
  
# Combine X and y into a single DataFrame
df = pd.concat([X, y], axis=1)

# Export to CSV
df.to_csv('Iris.csv', index=False)
```

The result for Iris is [Iris.csv](/logicGP.Tests/logicGP.Tests/Data/Real/Iris.csv). 

### Run AutoML

It is sufficient to run AutoML for one second if we only need the generated code:

```bash
mlnet classification --dataset "Iris.csv" --label-col 4 --has-header true --train-time 1 --name "IrisModel"
```

We only need the class ```ModelInput``` from ```IrisModel.consumption.cs``` and the method ```BuildPipeline``` from ```IrisModel.training.cs```.

### Write code for logicGP

[IrisTests](/logicGP.Tests/logicGP.Tests/Unit/Data/Real/IrisTests.cs) demonstrates the additional code needed to analyze the data with logicGP. Note, the following remarks.

### ```ModelInput```

```ModelInput``` can often be taken without modification. A ```float``` label for the output should however by changed to ```uint``` if possible.

### ```BuildPipeline```

```BuildPipeline``` needs some modification. 

#### No use of ```MapKeyToValue```

Due to internal restrictions of ML.NET, we cannot use a step like

```csharp
.Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName:@"PredictedLabel",inputColumnName:@"PredictedLabel"))
```

if the step is present (it does not seem possible to transfer the used mapping to Predicted Label).

#### Modified use of ```MapValueToKey```

For the same reason, the step

```csharp
.Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName:@"class",inputColumnName:@"class",addKeyValueAnnotationsAsText:false))
```

needs to be modified to use a custom mapping and - for the sake of simplicity - to map to a column called ```Label```. As a result, the unit test uses

```csharp
.Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",@"class", keyData: lookupIdvMap))
```

with 

```csharp
var lookupData = new[]
{
    new LookupMap<string>("Iris-setosa"),
    new LookupMap<string>("Iris-versicolor"),
    new LookupMap<string>("Iris-virginica")
};
var lookupIdvMap = mlContext.Data.LoadFromEnumerable(lookupData);
```

instead.

#### No direct support for Continuous values

Continuous values need to be binned for logicGP. The following example uses four bins:

```csharp
.Append(mlContext.Transforms.NormalizeBinning(new[]
            {
                new InputOutputColumnPair(@"sepal length", @"sepal length"),
                new InputOutputColumnPair(@"sepal width", @"sepal width"),
                new InputOutputColumnPair(@"petal length", @"petal length"),
                new InputOutputColumnPair(@"petal width", @"petal width")
            }, maximumBinCount: 4))
```

#### No support for One Hot Encoding

One hot encoded categorical string values get transforms like

```csharp
mlContext.Transforms.Categorical.OneHotEncoding(
            new[]
            {
                new InputOutputColumnPair(@"buying", @"buying"),
                new InputOutputColumnPair(@"maint", @"maint"),
                new InputOutputColumnPair(@"doors", @"doors"),
                new InputOutputColumnPair(@"persons", @"persons"),
                new InputOutputColumnPair(@"lug_boot", @"lug_boot"),
                new InputOutputColumnPair(@"safety", @"safety")
            })
```

They have to be transformed to manual mappings like

```csharp
mlContext.Transforms.Conversion.MapValue("buying",
            buyingLookupIdvMap, buyingLookupIdvMap.Schema["Category"],
            buyingLookupIdvMap.Schema["Value"], "buying")
...            
```                        

which uses maps defined by

```csharp
var buyingLookupData = new[]
        {
            new CategoryLookupMap { Value = 0f, Category = "low" },
            new CategoryLookupMap { Value = 1f, Category = "med" },
            new CategoryLookupMap { Value = 2f, Category = "high" },
            new CategoryLookupMap { Value = 3f, Category = "vhigh" }
        };
        var buyingLookupIdvMap =
            mlContext.Data.LoadFromEnumerable(buyingLookupData);
```            

