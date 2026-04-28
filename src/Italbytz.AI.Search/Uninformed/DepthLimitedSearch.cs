using Italbytz.AI;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Uninformed;

public class DepthLimitedSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
    where TState : notnull
{
    private readonly NodeFactory<TState, TAction> _nodeFactory = new();
    private readonly bool _avoidCycles;

    public DepthLimitedSearch(int limit, bool avoidCycles = true)
    {
        Limit = limit;
        _avoidCycles = avoidCycles;
        Metrics = new Metrics();
        _nodeFactory.AddNodeListener(_ => Metrics.IncrementInt(QueueSearch<TState, TAction>.MetricNodesExpanded));
        ClearMetrics();
    }

    public int Limit { get; }

    public IMetrics Metrics { get; }

    public bool LastSearchReachedCutoff { get; private set; }

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem)
    {
        _nodeFactory.UseParentLinks = true;
        var node = FindNode(problem);
        return SearchUtils.ToActions(node);
    }

    public TState? FindState(IProblem<TState, TAction> problem)
    {
        _nodeFactory.UseParentLinks = false;
        var node = FindNode(problem);
        return node is null ? default : node.State;
    }

    public INode<TState, TAction>? FindNode(IProblem<TState, TAction> problem)
    {
        ClearMetrics();
        LastSearchReachedCutoff = false;

        var root = _nodeFactory.CreateNode(problem.InitialState);
        var path = new HashSet<TState>();
        var outcome = RecursiveDls(root, problem, Limit, path);
        LastSearchReachedCutoff = outcome == SearchOutcome.Cutoff;
        return outcome == SearchOutcome.Success ? _solutionNode : null;
    }

    public bool IsCutoffResult(INode<TState, TAction>? node)
    {
        return node is null && LastSearchReachedCutoff;
    }

    private INode<TState, TAction>? _solutionNode;

    private SearchOutcome RecursiveDls(INode<TState, TAction> node, IProblem<TState, TAction> problem, int limit, HashSet<TState> path)
    {
        UpdateDepthMetrics(node.Depth + 1);

        if (problem.TestSolution(node))
        {
            Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, node.PathCost);
            _solutionNode = node;
            return SearchOutcome.Success;
        }

        if (limit == 0)
        {
            return SearchOutcome.Cutoff;
        }

        var cutoffOccurred = false;
        path.Add(node.State);

        foreach (var successor in _nodeFactory.GetSuccessors(node, problem))
        {
            if (_avoidCycles && path.Contains(successor.State))
            {
                continue;
            }

            var result = RecursiveDls(successor, problem, limit - 1, path);
            if (result == SearchOutcome.Cutoff)
            {
                cutoffOccurred = true;
            }
            else if (result == SearchOutcome.Success)
            {
                path.Remove(node.State);
                return SearchOutcome.Success;
            }
        }

        path.Remove(node.State);
        return cutoffOccurred ? SearchOutcome.Cutoff : SearchOutcome.Failure;
    }

    private void ClearMetrics()
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricNodesExpanded, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, 0);
        _solutionNode = null;
    }

    private void UpdateDepthMetrics(int activeDepth)
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, activeDepth);
        if (activeDepth > Metrics.GetInt(QueueSearch<TState, TAction>.MetricMaxQueueSize))
        {
            Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, activeDepth);
        }
    }

    private enum SearchOutcome
    {
        Success,
        Cutoff,
        Failure
    }
}