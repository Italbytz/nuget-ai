using System.Globalization;
using System.Text;

namespace Italbytz.AI.ML.Trainers.FastTree;

public static class RegressionTreeExtensions
{
    public static string ToGraphviz(this object tree)
    {
        var leafValues = GetDoubleListProperty(tree, "LeafValues");
        var splitFeatures = GetIntListProperty(tree, "NumericalSplitFeatureIndexes");
        var splitThresholds = GetDoubleListProperty(tree, "NumericalSplitThresholds");
        var leftChildren = GetIntListProperty(tree, "LeftChild");
        var rightChildren = GetIntListProperty(tree, "RightChild");

        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("    rankdir=\"TB\"");
        // Dark theme styling for black background
        sb.AppendLine("    bgcolor=\"transparent\"");
        sb.AppendLine("    fontcolor=\"white\"");
        sb.AppendLine("    node [fontcolor=\"white\", color=\"white\", style=\"filled\", fillcolor=\"#2a2a2a\"]");
        sb.AppendLine("    edge [fontcolor=\"white\", color=\"white\"]");

        for (var i = 0; i < leafValues.Count; i++)
        {
            var leafValue = leafValues[i].ToString("F2", CultureInfo.InvariantCulture);
            sb.AppendLine($"    l{i} [shape=box,label={leafValue}];");
        }

        for (var i = 0; i < splitFeatures.Count && i < splitThresholds.Count; i++)
        {
            var featureIndex = splitFeatures[i];
            var threshold = splitThresholds[i].ToString("F2", CultureInfo.InvariantCulture);
            sb.AppendLine($"    n{i} [shape=plain,label=<Feature{featureIndex}<br/>{threshold}>];");
        }

        for (var i = 0; i < leftChildren.Count && i < rightChildren.Count; i++)
        {
            var leftChildType = leftChildren[i] < 0 ? "l" : "n";
            var leftChildIndex = leftChildren[i] < 0 ? ~leftChildren[i] : leftChildren[i];
            var rightChildType = rightChildren[i] < 0 ? "l" : "n";
            var rightChildIndex = rightChildren[i] < 0 ? ~rightChildren[i] : rightChildren[i];

            sb.AppendLine($"    n{i} -> {leftChildType}{leftChildIndex} [label=\"<=\"];");
            sb.AppendLine($"    n{i} -> {rightChildType}{rightChildIndex} [label=\">\"];");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string ToPlantUml(this object tree)
    {
        var splitFeatures = GetIntListProperty(tree, "NumericalSplitFeatureIndexes");
        var splitThresholds = GetDoubleListProperty(tree, "NumericalSplitThresholds");
        var leftChildren = GetIntListProperty(tree, "LeftChild");
        var rightChildren = GetIntListProperty(tree, "RightChild");
        var leafValues = GetDoubleListProperty(tree, "LeafValues");

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("object RegressionTree {");
        sb.AppendLine($"    int[] NumericalSplitFeatureIndexes = [{string.Join(", ", splitFeatures)}]");

        var numericalSplitThresholdsStrings = splitThresholds
            .Select(v => v.ToString("F2", CultureInfo.InvariantCulture))
            .ToArray();
        sb.AppendLine($"    double[] NumericalSplitThresholds = [{string.Join(", ", numericalSplitThresholdsStrings)}]");
        sb.AppendLine($"    int[] LeftChild = [{string.Join(", ", leftChildren)}]");
        sb.AppendLine($"    int[] RightChild = [{string.Join(", ", rightChildren)}]");

        var leafValueStrings = leafValues
            .Select(v => v.ToString("F2", CultureInfo.InvariantCulture))
            .ToArray();
        sb.AppendLine($"    double[] LeafValues = [{string.Join(", ", leafValueStrings)}]");
        sb.AppendLine("}");
        sb.AppendLine("@enduml");
        return sb.ToString();
    }

    private static IReadOnlyList<int> GetIntListProperty(object instance, string propertyName)
    {
        var raw = instance.GetType().GetProperty(propertyName)?.GetValue(instance) as System.Collections.IEnumerable;
        if (raw == null)
        {
            return Array.Empty<int>();
        }

        var values = new List<int>();
        foreach (var item in raw)
        {
            if (item == null)
            {
                continue;
            }

            values.Add(Convert.ToInt32(item, CultureInfo.InvariantCulture));
        }

        return values;
    }

    private static IReadOnlyList<double> GetDoubleListProperty(object instance, string propertyName)
    {
        var raw = instance.GetType().GetProperty(propertyName)?.GetValue(instance) as System.Collections.IEnumerable;
        if (raw == null)
        {
            return Array.Empty<double>();
        }

        var values = new List<double>();
        foreach (var item in raw)
        {
            if (item == null)
            {
                continue;
            }

            values.Add(Convert.ToDouble(item, CultureInfo.InvariantCulture));
        }

        return values;
    }
}
