using System;
using System.Globalization;
using System.Linq;

namespace Italbytz.AI.Learning.Learners;

/// <summary>
/// Multiple linear regression learner using the normal equations β = (XᵀX)⁻¹Xᵀy.
/// Works with numeric-only datasets; the target attribute is the regression output.
/// </summary>
public class LinearRegressionLearner : ILearner
{
    private double[] _coefficients = [];
    private string[] _featureNames = [];
    private string _targetName = string.Empty;

    /// <summary>Fitted coefficients [bias, β₁, β₂, …] after training.</summary>
    public double[] Coefficients => _coefficients;

    public string[] FeatureNames => _featureNames;

    public void Train(IDataSet ds)
    {
        _targetName = ds.Specification.TargetAttribute;
        _featureNames = ds.GetNonTargetAttributes().ToArray();

        var n = ds.Examples.Count;
        var p = _featureNames.Length;

        // Build design matrix X (n × (p+1)) with a leading bias column of ones
        var x = new double[n][];
        var y = new double[n];

        for (var i = 0; i < n; i++)
        {
            var example = ds.Examples[i];
            var row = new double[p + 1];
            row[0] = 1.0; // bias
            for (var j = 0; j < p; j++)
                row[j + 1] = double.Parse(
                    example.GetAttributeValueAsString(_featureNames[j]),
                    CultureInfo.InvariantCulture);
            x[i] = row;
            y[i] = double.Parse(
                example.GetAttributeValueAsString(_targetName),
                CultureInfo.InvariantCulture);
        }

        // Normal equations: β = (XᵀX)⁻¹Xᵀy  — solved via Gaussian elimination
        var xtx = MultiplyTransposedByMatrix(x, p + 1);
        var xty = MultiplyTransposedByVector(x, y, p + 1);
        _coefficients = SolveLinearSystem(xtx, xty);
    }

    public string Predict(IExample e)
    {
        var value = PredictDouble(e);
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    public string[] Predict(IDataSet ds)
        => ds.Examples.Select(Predict).ToArray();

    /// <summary>
    /// Returns [within-tolerance, outside-tolerance] where tolerance = ±2.0 of the actual value.
    /// </summary>
    public int[] Test(IDataSet ds)
    {
        const double tolerance = 2.0;
        var correct = 0;
        var incorrect = 0;

        foreach (var example in ds.Examples)
        {
            var predicted = PredictDouble(example);
            var actual = double.Parse(
                example.GetAttributeValueAsString(_targetName),
                CultureInfo.InvariantCulture);

            if (Math.Abs(predicted - actual) <= tolerance)
                correct++;
            else
                incorrect++;
        }

        return [correct, incorrect];
    }

    /// <summary>R² coefficient of determination on the given dataset.</summary>
    public double RSquared(IDataSet ds)
    {
        var actuals = ds.Examples
            .Select(e => double.Parse(
                e.GetAttributeValueAsString(_targetName),
                CultureInfo.InvariantCulture))
            .ToArray();
        var predictions = ds.Examples.Select(PredictDouble).ToArray();

        var mean = actuals.Average();
        var ssTot = actuals.Sum(a => (a - mean) * (a - mean));
        var ssRes = actuals.Zip(predictions, (a, p) => (a - p) * (a - p)).Sum();

        return ssTot < 1e-10 ? 1.0 : 1.0 - ssRes / ssTot;
    }

    /// <summary>Mean absolute error on the given dataset.</summary>
    public double MeanAbsoluteError(IDataSet ds)
    {
        var n = ds.Examples.Count;
        if (n == 0)
            return 0.0;

        return ds.Examples.Sum(e =>
        {
            var predicted = PredictDouble(e);
            var actual = double.Parse(
                e.GetAttributeValueAsString(_targetName),
                CultureInfo.InvariantCulture);
            return Math.Abs(predicted - actual);
        }) / n;
    }

    private double PredictDouble(IExample e)
    {
        var value = _coefficients[0]; // bias
        for (var j = 0; j < _featureNames.Length; j++)
            value += _coefficients[j + 1] *
                     double.Parse(
                         e.GetAttributeValueAsString(_featureNames[j]),
                         CultureInfo.InvariantCulture);
        return value;
    }

    // XᵀX  (m × m where m = number of columns in X)
    private static double[][] MultiplyTransposedByMatrix(double[][] x, int m)
    {
        var n = x.Length;
        var result = new double[m][];
        for (var i = 0; i < m; i++)
        {
            result[i] = new double[m];
            for (var j = 0; j < m; j++)
                for (var k = 0; k < n; k++)
                    result[i][j] += x[k][i] * x[k][j];
        }
        return result;
    }

    // Xᵀy  (m-vector)
    private static double[] MultiplyTransposedByVector(double[][] x, double[] y, int m)
    {
        var n = x.Length;
        var result = new double[m];
        for (var i = 0; i < m; i++)
            for (var k = 0; k < n; k++)
                result[i] += x[k][i] * y[k];
        return result;
    }

    // Gaussian elimination with partial pivoting
    private static double[] SolveLinearSystem(double[][] a, double[] b)
    {
        var n = b.Length;

        // Augmented matrix [A | b]
        var aug = new double[n][];
        for (var i = 0; i < n; i++)
        {
            aug[i] = new double[n + 1];
            Array.Copy(a[i], aug[i], n);
            aug[i][n] = b[i];
        }

        for (var col = 0; col < n; col++)
        {
            // Partial pivot
            var maxRow = col;
            for (var row = col + 1; row < n; row++)
                if (Math.Abs(aug[row][col]) > Math.Abs(aug[maxRow][col]))
                    maxRow = row;
            (aug[col], aug[maxRow]) = (aug[maxRow], aug[col]);

            if (Math.Abs(aug[col][col]) < 1e-12)
                continue;

            for (var row = col + 1; row < n; row++)
            {
                var factor = aug[row][col] / aug[col][col];
                for (var j = col; j <= n; j++)
                    aug[row][j] -= factor * aug[col][j];
            }
        }

        // Back substitution
        var x = new double[n];
        for (var i = n - 1; i >= 0; i--)
        {
            x[i] = aug[i][n];
            for (var j = i + 1; j < n; j++)
                x[i] -= aug[i][j] * x[j];
            if (Math.Abs(aug[i][i]) > 1e-12)
                x[i] /= aug[i][i];
        }

        return x;
    }
}
