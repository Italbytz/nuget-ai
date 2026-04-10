using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Framework.QSearch;

/// <summary>
/// Standard tree search implementation.
/// </summary>
public class TreeSearch<TState, TAction> : QueueSearch<TState, TAction>
{
    public TreeSearch() : this(new NodeFactory<TState, TAction>())
    {
    }

    protected TreeSearch(NodeFactory<TState, TAction> nodeFactory) : base(nodeFactory)
    {
    }

    public override INode<TState, TAction>? FindNode(IProblem<TState, TAction> problem, NodePriorityQueue<TState, TAction> frontier)
    {
        ClearMetrics();
        var root = NodeFactory.CreateNode(problem.InitialState);
        AddToFrontier(frontier, root);

        if (EarlyGoalTest && IsGoal(root, problem))
        {
            return root;
        }

        while (!IsFrontierEmpty(frontier))
        {
            var node = RemoveFromFrontier(frontier);
            if (IsGoal(node, problem))
            {
                return node;
            }

            foreach (var successor in NodeFactory.GetSuccessors(node, problem))
            {
                AddToFrontier(frontier, successor);
                if (EarlyGoalTest && IsGoal(successor, problem))
                {
                    return successor;
                }
            }
        }

        return null;
    }

    protected virtual bool IsFrontierEmpty(NodePriorityQueue<TState, TAction> frontier)
    {
        return frontier.Count == 0;
    }

    protected virtual void AddToFrontier(NodePriorityQueue<TState, TAction> frontier, INode<TState, TAction> node)
    {
        frontier.Enqueue(node);
        UpdateMetrics(frontier.Count);
    }

    protected virtual INode<TState, TAction> RemoveFromFrontier(NodePriorityQueue<TState, TAction> frontier)
    {
        var result = frontier.Dequeue();
        UpdateMetrics(frontier.Count);
        return result;
    }

    private bool IsGoal(INode<TState, TAction> node, IProblem<TState, TAction> problem)
    {
        if (!problem.TestSolution(node))
        {
            return false;
        }

        Metrics.Set(MetricPathCost, node.PathCost);
        return true;
    }
}
