using Italbytz.AI.Search.Framework;

namespace Italbytz.AI.Search.Demos.WeightedGraph;

public sealed class WeightedGraphSearchSimulator
{
    public WeightedGraphSearchResult SimulateDijkstra(
        string startNode,
        IReadOnlyList<string> vertices,
        IReadOnlyList<WeightedGraphEdge> edges)
    {
        var frontier = new List<FrontierItem>();
        var explored = new HashSet<string>();
        var distances = vertices.ToDictionary(vertex => vertex, _ => double.PositiveInfinity);
        var predecessors = vertices.ToDictionary(vertex => vertex, _ => (string?)null);
        var adjacency = BuildAdjacency(edges);
        var steps = new List<WeightedGraphSearchStep>();
        long nextSequence = 0;

        distances[startNode] = 0;
        frontier.Add(new FrontierItem(new Node<string, WeightedGraphMoveToAction>(startNode), 0, nextSequence++));

        while (frontier.Count > 0)
        {
            frontier.Sort(FrontierItemComparer.Instance);

            var current = frontier[0];
            frontier.RemoveAt(0);

            if (!explored.Add(current.Node.State))
            {
                continue;
            }

            var relaxations = new List<WeightedGraphRelaxation>();
            foreach (var edge in adjacency.GetValueOrDefault(current.Node.State, []))
            {
                var successorState = edge.ToNode;
                if (explored.Contains(successorState))
                {
                    continue;
                }

                var candidateDistance = current.Node.PathCost + edge.Cost;
                var accepted = candidateDistance < distances[successorState];
                relaxations.Add(new WeightedGraphRelaxation(current.Node.State, successorState, candidateDistance, accepted));

                if (!accepted)
                {
                    continue;
                }

                distances[successorState] = candidateDistance;
                predecessors[successorState] = current.Node.State;

                var successorNode = new Node<string, WeightedGraphMoveToAction>(
                    successorState,
                    current.Node,
                    new WeightedGraphMoveToAction(successorState, edge.Cost),
                    candidateDistance);

                frontier.Add(new FrontierItem(successorNode, candidateDistance, nextSequence++));
            }

            steps.Add(new WeightedGraphSearchStep(
                steps.Count + 1,
                ToTraceNode(current),
                CreateFrontierSnapshot(frontier),
                CreateDistanceSnapshot(vertices, distances, predecessors),
                relaxations));
        }

        return new WeightedGraphSearchResult(
            CreateDistanceSnapshot(vertices, distances, predecessors),
            CreateRoutes(startNode, vertices, distances, predecessors),
            steps);
    }

    private static Dictionary<string, List<WeightedGraphEdge>> BuildAdjacency(IReadOnlyList<WeightedGraphEdge> edges)
    {
        var adjacency = new Dictionary<string, List<WeightedGraphEdge>>(StringComparer.Ordinal);

        foreach (var edge in edges)
        {
            AddDirectedEdge(adjacency, edge.FromNode, edge.ToNode, edge.Cost);
            AddDirectedEdge(adjacency, edge.ToNode, edge.FromNode, edge.Cost);
        }

        return adjacency;
    }

    private static void AddDirectedEdge(Dictionary<string, List<WeightedGraphEdge>> adjacency, string fromNode, string toNode, double cost)
    {
        if (!adjacency.TryGetValue(fromNode, out var outgoingEdges))
        {
            outgoingEdges = [];
            adjacency[fromNode] = outgoingEdges;
        }

        outgoingEdges.Add(new WeightedGraphEdge(fromNode, toNode, cost));
    }

    private static WeightedGraphTraceNode ToTraceNode(FrontierItem item)
        => new(item.Node.State, item.Node.PathCost, item.Priority, item.Node.Depth);

    private static IReadOnlyList<WeightedGraphTraceNode> CreateFrontierSnapshot(IReadOnlyCollection<FrontierItem> frontier)
        => frontier
            .OrderBy(item => item.Priority)
            .ThenBy(item => item.Sequence)
            .GroupBy(item => item.Node.State, StringComparer.Ordinal)
            .Select(group => ToTraceNode(group.First()))
            .ToArray();

    private static IReadOnlyList<WeightedGraphDistanceState> CreateDistanceSnapshot(
        IReadOnlyList<string> vertices,
        IReadOnlyDictionary<string, double> distances,
        IReadOnlyDictionary<string, string?> predecessors)
        => vertices.Select(vertex => new WeightedGraphDistanceState(
            vertex,
            double.IsPositiveInfinity(distances[vertex]) ? null : distances[vertex],
            predecessors[vertex]))
        .ToArray();

    private static IReadOnlyList<WeightedGraphRoute> CreateRoutes(
        string startNode,
        IReadOnlyList<string> vertices,
        IReadOnlyDictionary<string, double> distances,
        IReadOnlyDictionary<string, string?> predecessors)
        => vertices
            .Where(vertex => !vertex.Equals(startNode, StringComparison.Ordinal) && !double.IsPositiveInfinity(distances[vertex]))
            .Select(vertex => new WeightedGraphRoute(vertex, ReconstructPath(startNode, vertex, predecessors), distances[vertex]))
            .ToArray();

    private static IReadOnlyList<string> ReconstructPath(string startNode, string targetNode, IReadOnlyDictionary<string, string?> predecessors)
    {
        var path = new Stack<string>();
        string? current = targetNode;

        while (current is not null)
        {
            path.Push(current);
            if (current.Equals(startNode, StringComparison.Ordinal))
            {
                break;
            }

            current = predecessors[current];
        }

        return path.ToArray();
    }

    private sealed record FrontierItem(INode<string, WeightedGraphMoveToAction> Node, double Priority, long Sequence);

    private sealed class FrontierItemComparer : IComparer<FrontierItem>
    {
        public static FrontierItemComparer Instance { get; } = new();

        public int Compare(FrontierItem? left, FrontierItem? right)
        {
            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            var priorityComparison = left.Priority.CompareTo(right.Priority);
            return priorityComparison != 0 ? priorityComparison : left.Sequence.CompareTo(right.Sequence);
        }
    }
}