namespace Italbytz.AI.ML.Core;

public class Metric
{
    public double Accuracy { get; set; }

    public double AreaUnderRocCurve { get; set; }

    public double AreaUnderPrecisionRecallCurve { get; set; }

    public double F1Score { get; set; }

    public double LogLoss { get; set; }

    public double MeanAbsoluteError { get; set; }

    public double MeanSquaredError { get; set; }

    public double RootMeanSquaredError { get; set; }

    public double R2Score { get; set; }

    public double SpearmanCorrelationCoefficient { get; set; }

    public double MacroAccuracy { get; set; }

    public double MicroAccuracy { get; set; }
}
