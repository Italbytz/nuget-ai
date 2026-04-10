using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning;

internal static class LearningUtils
{
    public static List<T> RemoveFrom<T>(IEnumerable<T> list, T member)
    {
        var newList = new List<T>(list);
        newList.Remove(member);
        return newList;
    }

    public static List<double> Normalize(List<double> values)
    {
        var total = values.Sum();
        if (Math.Abs(total) < double.Epsilon)
        {
            return values.Select(_ => 0.0).ToList();
        }

        return values.Select(value => value / total).ToList();
    }

    public static T? Mode<T>(IEnumerable<T> values) where T : notnull
    {
        return values
            .GroupBy(value => value)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault();
    }

    public static double Information(IEnumerable<double> probabilities)
    {
        return probabilities.Where(probability => probability > 0)
            .Sum(probability => -probability * Math.Log(probability, 2));
    }
}
