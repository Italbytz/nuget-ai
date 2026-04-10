using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Framework.QSearch;

/// <summary>
/// Shared queue-search infrastructure with basic search metrics.
/// </summary>
public abstract class QueueSearch<TState, TAction>
{
    public const string MetricNodesExpanded = "nodesExpanded";
    public const string MetricQueueSize = "queueSize";
    public const string MetricMaxQueueSize = "maxQueueSize";
    public const string MetricPathCost = "pathCost";

    protected QueueSearch(NodeFactory<TState, TAction> nodeFactory)
    {
        NodeFactory = nodeFactory;
        NodeFactory.AddNodeListener(_ => Metrics.IncrementInt(MetricNodesExpanded));
        ClearMetrics();
    }

    public NodeFactory<TState, TAction> NodeFactory { get; }

    protected bool EarlyGoalTest => false;

    public IMetrics Metrics { get; } = new Metrics();

    public abstract INode<TState, TAction>? FindNode(IProblem<TState, TAction> problem, NodePriorityQueue<TState, TAction> frontier);

    protected void ClearMetrics()
    {
        Metrics.Set(MetricNodesExpanded, 0);
        Metrics.Set(MetricQueueSize, 0);
        Metrics.Set(MetricMaxQueueSize, 0);
        Metrics.Set(MetricPathCost, 0);
    }

    protected void UpdateMetrics(int queueSize)
    {
        Metrics.Set(MetricQueueSize, queueSize);
        var maxQueueSize = Metrics.GetInt(MetricMaxQueueSize);
        if (queueSize > maxQueueSize)
        {
            Metrics.Set(MetricMaxQueueSize, queueSize);
        }
    }
}
