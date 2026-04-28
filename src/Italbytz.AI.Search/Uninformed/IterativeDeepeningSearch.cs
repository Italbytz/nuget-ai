using Italbytz.AI;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Uninformed;

public class IterativeDeepeningSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
    where TState : notnull
{
    public IMetrics Metrics { get; } = new Metrics();

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem)
    {
        var node = FindNode(problem, useParentLinks: true);
        return SearchUtils.ToActions(node);
    }

    public TState? FindState(IProblem<TState, TAction> problem)
    {
        var node = FindNode(problem, useParentLinks: false);
        return node is null ? default : node.State;
    }

    private INode<TState, TAction>? FindNode(IProblem<TState, TAction> problem, bool useParentLinks)
    {
        ClearMetrics();

        for (var depth = 0; depth < int.MaxValue; depth++)
        {
            var dls = new DepthLimitedSearch<TState, TAction>(depth);
            var node = dls.FindNode(problem);

            Metrics.Set(QueueSearch<TState, TAction>.MetricNodesExpanded,
                Metrics.GetInt(QueueSearch<TState, TAction>.MetricNodesExpanded) + dls.Metrics.GetInt(QueueSearch<TState, TAction>.MetricNodesExpanded));
            Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize,
                Math.Max(Metrics.GetInt(QueueSearch<TState, TAction>.MetricMaxQueueSize), dls.Metrics.GetInt(QueueSearch<TState, TAction>.MetricMaxQueueSize)));

            if (!dls.IsCutoffResult(node))
            {
                Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, dls.Metrics.GetInt(QueueSearch<TState, TAction>.MetricQueueSize));
                Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, dls.Metrics.GetDouble(QueueSearch<TState, TAction>.MetricPathCost));
                return useParentLinks ? node : (node is null ? null : new Node<TState, TAction>(node.State));
            }
        }

        return null;
    }

    private void ClearMetrics()
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricNodesExpanded, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, 0);
    }
}