using Microsoft.ML.Data;

namespace Italbytz.AI.ML.Core;

public interface IScoreScalar
{
    float Score { get; set; }
}

public interface IScoreVector
{
    VBuffer<float> Score { get; set; }
}

public interface IProbabilityScalar
{
    float Probability { get; set; }
}

public interface IProbabilityVector
{
    VBuffer<float> Probability { get; set; }
}

public interface IFeatures
{
    float[] Features { get; set; }
}

public interface IPredictedLabel
{
    uint PredictedLabel { get; set; }
}

public interface ICustomMappingInput : IFeatures
{
}

public class CustomMappingInput : ICustomMappingInput
{
    public float[] Features { get; set; } = [];
}

public class ClassificationInput : CustomMappingInput
{
}

public class RegressionInput : CustomMappingInput
{
}

public interface ICustomMappingOutput
{
}

public interface IRegressionOutput : ICustomMappingOutput, IScoreScalar
{
}

public interface IClassificationOutput : ICustomMappingOutput, IPredictedLabel
{
}

public interface IBinaryClassificationOutput : IClassificationOutput, IScoreScalar, IProbabilityScalar
{
}

public interface IMulticlassClassificationOutput : IClassificationOutput, IScoreVector, IProbabilityVector
{
}

public class RegressionOutput : IRegressionOutput
{
    public float Score { get; set; }
}

public class BinaryClassificationOutput : IBinaryClassificationOutput
{
    [KeyType(2)]
    public uint PredictedLabel { get; set; }

    public float Score { get; set; }

    public float Probability { get; set; }
}

public class MulticlassClassificationOutput : IMulticlassClassificationOutput
{
    public uint PredictedLabel { get; set; }

    [VectorType(2)]
    public virtual VBuffer<float> Score { get; set; }

    public VBuffer<float> Probability { get; set; }
}
