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

    [VectorType]
    public virtual VBuffer<float> Score { get; set; }

    [VectorType]
    public VBuffer<float> Probability { get; set; }
}

public class TernaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(3)]
    public override VBuffer<float> Score { get; set; }
}

public class QuaternaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(4)]
    public override VBuffer<float> Score { get; set; }
}

public class QuinaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(5)]
    public override VBuffer<float> Score { get; set; }
}

public class SenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(6)]
    public override VBuffer<float> Score { get; set; }
}

public class SeptenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(7)]
    public override VBuffer<float> Score { get; set; }
}

public class OctonaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(8)]
    public override VBuffer<float> Score { get; set; }
}

public class NonaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(9)]
    public override VBuffer<float> Score { get; set; }
}

public class DenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(10)]
    public override VBuffer<float> Score { get; set; }
}

public class UndenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(11)]
    public override VBuffer<float> Score { get; set; }
}

public class DuodenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(12)]
    public override VBuffer<float> Score { get; set; }
}

public class TridenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(13)]
    public override VBuffer<float> Score { get; set; }
}

public class TetradenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(14)]
    public override VBuffer<float> Score { get; set; }
}

public class PentadenaryClassificationOutput : MulticlassClassificationOutput
{
    [VectorType(15)]
    public override VBuffer<float> Score { get; set; }
}
