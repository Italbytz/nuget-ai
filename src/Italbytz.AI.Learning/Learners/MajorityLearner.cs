using System;
using System.Linq;

namespace Italbytz.AI.Learning.Learners;

public class MajorityLearner : ILearner
{
    private string _result = string.Empty;

    public void Train(IDataSet ds)
    {
        var targets = ds.Examples.Select(example => example.TargetValue()).ToList();
        _result = LearningUtils.Mode(targets) ?? throw new InvalidOperationException("Dataset did not contain any target values.");
    }

    public string[] Predict(IDataSet ds)
    {
        return ds.Examples.Select(Predict).ToArray();
    }

    public string Predict(IExample e)
    {
        return _result;
    }

    public int[] Test(IDataSet ds)
    {
        var results = new[] { 0, 0 };
        foreach (var example in ds.Examples)
        {
            if (example.TargetValue().Equals(_result, StringComparison.Ordinal))
            {
                results[0]++;
            }
            else
            {
                results[1]++;
            }
        }

        return results;
    }
}
