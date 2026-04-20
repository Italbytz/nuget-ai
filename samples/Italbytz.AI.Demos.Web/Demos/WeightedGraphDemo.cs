using Italbytz.AI.Search.Demos.WeightedGraph;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record WeightedGraphScenario(
    string Key,
    string Name,
    string Summary,
    string StartNode,
    IReadOnlyList<string> Vertices,
    IReadOnlyList<WeightedGraphEdge> Edges);

internal static class WeightedGraphDemoFactory
{
    public static IReadOnlyList<WeightedGraphScenario> BuildScenarios()
    {
        return
        [
            new WeightedGraphScenario(
                "Campus",
                "Campus graph",
                "A compact graph where the shortest route to the far node requires one late relaxation through an intermediate vertex.",
                "A",
                ["A", "B", "C", "D", "E", "F"],
                [
                    new WeightedGraphEdge("A", "B", 4),
                    new WeightedGraphEdge("A", "C", 1),
                    new WeightedGraphEdge("C", "B", 2),
                    new WeightedGraphEdge("B", "D", 1),
                    new WeightedGraphEdge("C", "D", 5),
                    new WeightedGraphEdge("D", "E", 3),
                    new WeightedGraphEdge("B", "F", 6),
                    new WeightedGraphEdge("E", "F", 2)
                ]),
            new WeightedGraphScenario(
                "Depot",
                "Depot graph",
                "Shows how a locally expensive edge can still belong to the global optimum when it connects into a cheaper suffix.",
                "A",
                ["A", "B", "C", "D", "E", "F"],
                [
                    new WeightedGraphEdge("A", "B", 3),
                    new WeightedGraphEdge("A", "C", 6),
                    new WeightedGraphEdge("B", "C", 2),
                    new WeightedGraphEdge("B", "D", 5),
                    new WeightedGraphEdge("C", "E", 2),
                    new WeightedGraphEdge("D", "E", 1),
                    new WeightedGraphEdge("D", "F", 4),
                    new WeightedGraphEdge("E", "F", 1)
                ]),
            new WeightedGraphScenario(
                "Ring",
                "Ring graph",
                "Useful for showing frontier tie-breaking and why already explored nodes are not relaxed again.",
                "A",
                ["A", "B", "C", "D", "E", "F"],
                [
                    new WeightedGraphEdge("A", "B", 2),
                    new WeightedGraphEdge("B", "C", 2),
                    new WeightedGraphEdge("C", "D", 2),
                    new WeightedGraphEdge("D", "E", 2),
                    new WeightedGraphEdge("E", "F", 2),
                    new WeightedGraphEdge("F", "A", 2),
                    new WeightedGraphEdge("A", "D", 5),
                    new WeightedGraphEdge("B", "E", 4)
                ])
        ];
    }

    public static WeightedGraphScenario CreateRandomScenario(Random random)
    {
        return BuildScenarios()[random.Next(BuildScenarios().Count)];
    }

    public static string FormatDistance(double? distance)
    {
        return distance.HasValue
            ? distance.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
            : "inf";
    }
}