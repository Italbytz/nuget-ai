namespace Italbytz.AI.ML.Core.Configuration;

public enum DataSourceType
{
    TabularFile,
    SQL,
    Folder,
    Vott,
    Coco
}

public enum EnvironmentType
{
    LocalCPU,
    LocalGPU,
    Azure
}

public enum ScenarioType
{
    Default,
    Classification,
    Regression,
    Recommendation,
    ImageClassification,
    ObjectDetection,
    Forecasting,
    TextClassification,
    SentenceSimilarity,
    QuestionAnswer,
    NamedEntityRecognition
}

public enum AutoMLType
{
    NNI = 1,
    AutoML = 2,
    Octopus = 3
}

public enum EstimatorType
{
    LightGbmBinary,
    LightGbmMulti,
    LightGbmRegression,
    FastForestBinary,
    FastForestOva,
    FastForestRegression,
    FastTreeBinary,
    FastTreeOva,
    FastTreeRegression,
    FastTreeTweedieRegression,
    LbfgsLogisticRegressionBinary,
    LbfgsLogisticRegressionOva,
    LbfgsMaximumEntropyMulti,
    LbfgsPoissonRegressionRegression,
    SdcaLogisticRegressionBinary,
    SdcaLogisticRegressionOva,
    SdcaMaximumEntropyMulti,
    SdcaRegression,
    MatrixFactorization,
    ImageClassificationMulti,
    TextClassificationMulti,
    SentenceSimilarityRegression,
    ObjectDetectionMulti,
    QuestionAnsweringMulti,
    NamedEntityRecognitionMulti,
    ForecastBySsa,
    Concatenate,
    Naive,
    OneHotEncoding,
    OneHotHashEncoding,
    LoadRawImageBytes,
    MapKeyToValue,
    ReplaceMissingValues,
    NormalizeMinMax,
    FeaturizeText,
    NormalizeText,
    ConvertType,
    MapValueToKey,
    ApplyOnnxModel,
    ResizeImages,
    ExtractPixels,
    LoadImages,
    DnnFeaturizerImage,
    Unknown
}
