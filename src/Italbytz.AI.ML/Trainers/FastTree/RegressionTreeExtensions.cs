using System.Globalization;
using System.Text;
using Microsoft.ML.Trainers.FastTree;

namespace Italbytz.AI.ML.Trainers.FastTree;

public static class RegressionTreeExtensions
{
    public static string ToGraphviz(this RegressionTree tree)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("    rankdir=\"TB\"");
        // Dark theme styling for black background
        sb.AppendLine("    bgcolor=\"transparent\"");
        sb.AppendLine("    fontcolor=\"white\"");
        sb.AppendLine("    node [fontcolor=\"white\", color=\"white\", style=\"filled\", fillcolor=\"#2a2a2a\"]");
        sb.AppendLine("    edge [fontcolor=\"white\", color=\"white\"]");

        for (var i = 0; i < tree.LeafValues.Count; i++)
        {
            var leafValue = tree.LeafValues[i].ToString("F2", CultureInfo.InvariantCulture);
            sb.AppendLine($"    l{i} [shape=box,label={leafValue}];");
        }

        for (var i = 0; i < tree.NumericalSplitFeatureIndexes.Count; i++)
        {
            var featureIndex = tree.NumericalSplitFeatureIndexes[i];
            var threshold = tree.NumericalSplitThresholds[i].ToString("F2", CultureInfo.InvariantCulture);
            sb.AppendLine($"    n{i} [shape=plain,label=<Feature{featureIndex}<br/>{threshold}>];");
        }

        for (var i = 0; i < tree.LeftChild.Count; i++)
        {
            var leftChildType = tree.LeftChild[i] < 0 ? "l" : "n";
            var leftChildIndex = tree.LeftChild[i] < 0 ? ~tree.LeftChild[i] : tree.LeftChild[i];
            var rightChildType = tree.RightChild[i] < 0 ? "l" : "n";
            var rightChildIndex = tree.RightChild[i] < 0 ? ~tree.RightChild[i] : tree.RightChild[i];

            sb.AppendLine($"    n{i} -> {leftChildType}{leftChildIndex} [label=\"<=\"];");
            sb.AppendLine($"    n{i} -> {rightChildType}{rightChildIndex} [label=\">\"];");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string ToPlantUml(this RegressionTree tree)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("object RegressionTree {");
        sb.AppendLine($"    int[] NumericalSplitFeatureIndexes = [{string.Join(", ", tree.NumericalSplitFeatureIndexes)}]");

        var numericalSplitThresholdsStrings = tree.NumericalSplitThresholds
            .Select(v => v.ToString("F2", CultureInfo.InvariantCulture))
            .ToArray();
        sb.AppendLine($"    double[] NumericalSplitThresholds = [{string.Join(", ", numericalSplitThresholdsStrings)}]");
        sb.AppendLine($"    int[] LeftChild = [{string.Join(", ", tree.LeftChild)}]");
        sb.AppendLine($"    int[] RightChild = [{string.Join(", ", tree.RightChild)}]");

        var leafValueStrings = tree.LeafValues
            .Select(v => v.ToString("F2", CultureInfo.InvariantCulture))
            .ToArray();
        sb.AppendLine($"    double[] LeafValues = [{string.Join(", ", leafValueStrings)}]");
        sb.AppendLine("}");
        sb.AppendLine("@enduml");
        return sb.ToString();
    }
}
