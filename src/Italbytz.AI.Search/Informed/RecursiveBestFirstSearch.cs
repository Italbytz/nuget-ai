using Italbytz.AI;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Informed;

/// <summary>
/// Recursive best-first search using an A*-style evaluation function.
/// </summary>
public class RecursiveBestFirstSearch<TState, TAction> : ISearchForActions<TState, TAction>
    where TState : notnull
{
    public const string MetricNodesExpanded = "nodesExpanded";
    public const string MetricMaxRecursiveDepth = "maxRecursiveDepth";
    public const string MetricPathCost = "pathCost";

    private readonly Func<INode<TState, TAction>, double> _evaluation;
    private readonly INodeFactory<TState, TAction> _nodeFactory;
    private readonly bool _avoidLoops;
    private readonly HashSet<TState> _explored = [];

    public RecursiveBestFirstSearch(Func<INode<TState, TAction>, double> heuristic)
        : this(heuristic, avoidLoops: false)
    {
    }

    public RecursiveBestFirstSearch(Func<INode<TState, TAction>, double> heuristic, bool avoidLoops)
        : this(heuristic, avoidLoops, new NodeFactory<TState, TAction>())
    {
    }

    public RecursiveBestFirstSearch(
        Func<INode<TState, TAction>, double> heuristic,
        bool avoidLoops,
        INodeFactory<TState, TAction> nodeFactory)
    {
        _avoidLoops = avoidLoops;
        _nodeFactory = nodeFactory;
        _evaluation = node => node.PathCost + heuristic(node);
        _nodeFactory.AddNodeListener(_ => Metrics.IncrementInt(MetricNodesExpanded));
    }

    public IMetrics Metrics { get; } = new Metrics();

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem)
    {
        _explored.Clear();
        ClearMetrics();

        var start = _nodeFactory.CreateNode(problem.InitialState);
        var result = Search(problem, start, _evaluation(start), double.PositiveInfinity, 0);
        if (!result.HasSolution)
        {
            return [];
        }

        Metrics.Set(MetricPathCost, result.SolutionNode!.PathCost);
        return SearchUtils.ToActions(result.SolutionNode);
    }

    private SearchResult Search(
        IProblem<TState, TAction> problem,
        INode<TState, TAction> node,
        double nodeCost,
        double limit,
        int recursiveDepth)
    {
        UpdateRecursiveDepth(recursiveDepth);
        if (problem.TestSolution(node))
        {
            return new SearchResult(node, limit);
        }

        var successors = Expand(node, problem);
        if (successors.Count == 0)
        {
            return new SearchResult(null, double.PositiveInfinity);
        }

        var values = successors.Select(successor => Math.Max(_evaluation(successor), nodeCost)).ToArray();

        while (true)
        {
            var bestIndex = GetBestIndex(values);
            if (values[bestIndex] > limit)
            {
                return new SearchResult(null, values[bestIndex]);
            }

            var alternative = values[GetAlternativeIndex(values, bestIndex)];
            var result = Search(problem, successors[bestIndex], values[bestIndex], Math.Min(limit, alternative), recursiveDepth + 1);
            values[bestIndex] = result.FCostLimit;
            if (result.HasSolution)
            {
                return result;
            }
        }
    }

    private List<INode<TState, TAction>> Expand(INode<TState, TAction> node, IProblem<TState, TAction> problem)
    {
        var successors = _nodeFactory.GetSuccessors(node, problem);
        if (!_avoidLoops)
        {
            return successors;
        }

        _explored.Add(node.State);
        return successors.Where(candidate => !_explored.Contains(candidate.State)).ToList();
    }

    private void ClearMetrics()
    {
        Metrics.Set(MetricPathCost, 0);
        Metrics.Set(MetricNodesExpanded, 0);
        Metrics.Set(MetricMaxRecursiveDepth, 0);
    }

    private void UpdateRecursiveDepth(int recursiveDepth)
    {
        var currentMax = Metrics.GetInt(MetricMaxRecursiveDepth);
        if (recursiveDepth > currentMax)
        {
            Metrics.Set(MetricMaxRecursiveDepth, recursiveDepth);
        }
    }

    private static int GetBestIndex(IReadOnlyList<double> values)
    {
        var bestIndex = 0;
        var bestValue = values[0];
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index] < bestValue)
            {
                bestValue = values[index];
                bestIndex = index;
            }
        }

        return bestIndex;
    }

    private static int GetAlternativeIndex(IReadOnlyList<double> values, int bestIndex)
    {
        if (values.Count == 1)
        {
            return bestIndex;
        }

        var alternativeIndex = bestIndex == 0 ? 1 : 0;
        var alternativeValue = values[alternativeIndex];
        for (var index = 0; index < values.Count; index++)
        {
            if (index == bestIndex || values[index] >= alternativeValue)
            {
                continue;
            }

            alternativeValue = values[index];
            alternativeIndex = index;
        }

        return alternativeIndex;
    }

    private sealed record SearchResult(INode<TState, TAction>? SolutionNode, double FCostLimit)
    {
        public bool HasSolution => SolutionNode is not null;
    }
}