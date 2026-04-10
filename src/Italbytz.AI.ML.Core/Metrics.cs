namespace Italbytz.AI.ML.Core;

public class Metrics
{
    public bool IsRegression { get; set; }

    public double RSquared { get; set; }

    public double MeanAbsoluteError { get; set; }

    public double MeanSquaredError { get; set; }

    public double RootMeanSquaredError { get; set; }

    public bool IsBinaryClassification { get; set; }

    public bool IsMulticlassClassification { get; set; }

    public double MacroAccuracy { get; set; }

    public double Accuracy { get; set; }

    public double AreaUnderRocCurve { get; set; }

    public double F1Score { get; set; }

    public double AreaUnderPrecisionRecallCurve { get; set; }
}
