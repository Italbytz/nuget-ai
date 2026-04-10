using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.Trainers;

public class LeastSquaresTrainer : CustomRegressionTrainer
{
    private double[]? _parameters;

    protected override void PrepareForFit(IDataView input)
    {
        var features = input
            .GetColumn<float[]>(Core.DefaultColumnNames.Features)
            .ToList();
        var labels = input
            .GetColumn<float>(Core.DefaultColumnNames.Label)
            .ToList();

        var x = features
            .Select(feature => feature.Select(entry => (double)entry).ToArray())
            .ToArray();
        var y = labels.Select(label => (double)label).ToArray();

        _parameters = MathNet.Numerics.Fit.MultiDim(x, y, intercept: true);
    }

    protected override void Map(Core.RegressionInput input, Core.RegressionOutput output)
    {
        ArgumentNullException.ThrowIfNull(_parameters);

        var score = _parameters[0];
        var features = input.Features;
        for (var i = 1; i < _parameters.Length; i++)
        {
            score += _parameters[i] * features[i - 1];
        }

        output.Score = (float)score;
    }
}
