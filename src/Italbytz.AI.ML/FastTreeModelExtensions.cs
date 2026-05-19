using System.Reflection;
using Italbytz.AI.ML.Trainers;
using Italbytz.AI.ML.Trainers.FastTree;
using Microsoft.ML;

namespace Italbytz.AI.ML;

public static class FastTreeModelExtensions
{
    public static IReadOnlyList<object> ExtractFastTreeRegressionTrees(this ITransformer transformer)
    {
        var modelParameters = transformer.GetModelParameters();
        var trees = new List<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        CollectFastTreeParameters(modelParameters, trees, visited, 0);

        return trees;
    }

    public static IReadOnlyList<string> ExportFastTreeRegressionTreesAsGraphviz(this ITransformer transformer)
    {
        return transformer.ExtractFastTreeRegressionTrees().Select(tree => tree.ToGraphviz()).ToArray();
    }

    public static IReadOnlyList<string> ExportFastTreeRegressionTreesAsPlantUml(this ITransformer transformer)
    {
        return transformer.ExtractFastTreeRegressionTrees().Select(tree => tree.ToPlantUml()).ToArray();
    }

    private static void CollectFastTreeParameters(object? candidate, List<object> trees, HashSet<object> visited, int depth)
    {
        if (candidate == null || depth > 8)
        {
            return;
        }

        if (!visited.Add(candidate))
        {
            return;
        }

        var candidateType = candidate.GetType();
        if (string.Equals(candidateType.FullName, "Microsoft.ML.Trainers.FastTree.FastTreeBinaryModelParameters", StringComparison.Ordinal))
        {
            var ensemble = candidateType
                .GetProperty("TrainedTreeEnsemble", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(candidate);

            var treeEnumerable = ensemble?
                .GetType()
                .GetProperty("Trees", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(ensemble) as System.Collections.IEnumerable;

            if (treeEnumerable != null)
            {
                foreach (var tree in treeEnumerable)
                {
                    if (tree != null)
                    {
                        trees.Add(tree);
                    }
                }
            }

            return;
        }

        if (candidate is System.Collections.IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                CollectFastTreeParameters(item, trees, visited, depth + 1);
            }

            return;
        }

        var type = candidate.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(candidate);
            }
            catch
            {
                continue;
            }

            CollectFastTreeParameters(value, trees, visited, depth + 1);
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            object? value;
            try
            {
                value = field.GetValue(candidate);
            }
            catch
            {
                continue;
            }

            CollectFastTreeParameters(value, trees, visited, depth + 1);
        }
    }
}
