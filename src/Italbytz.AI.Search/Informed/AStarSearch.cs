using System.Collections.Generic;
using Italbytz.AI;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Informed;

/// <summary>
/// A* graph search that orders frontier nodes by path cost plus heuristic estimate.
/// </summary>
public class AStarSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
    where TState : notnull
{
    private readonly Func<TState, double> _heuristic;

    public AStarSearch(Func<TState, double> heuristic)
    {
        _heuristic = heuristic;
        ResetMetrics();
    }

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
        ResetMetrics();

        var frontier = new List<FrontierItem>();
        var explored = new HashSet<TState>();
        long nextSequence = 0;

        var root = new Node<TState, TAction>(problem.InitialState);
        frontier.Add(new FrontierItem(root, root.PathCost + _heuristic(root.State), nextSequence++));
        UpdateQueueMetrics(frontier.Count);

        while (frontier.Count > 0)
        {
            frontier.Sort(FrontierItemComparer.Instance);

            var current = frontier[0];
            frontier.RemoveAt(0);
            UpdateQueueMetrics(frontier.Count);

            if (!explored.Add(current.Node.State))
            {
                continue;
            }

            if (problem.TestSolution(current.Node))
            {
                Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, current.Node.PathCost);
                return current.Node;
            }

            Metrics.IncrementInt(QueueSearch<TState, TAction>.MetricNodesExpanded);

            foreach (var action in problem.Actions(current.Node.State))
            {
                var successorState = problem.Result(current.Node.State, action);
                if (explored.Contains(successorState))
                {
                    continue;
                }

                var stepCost = problem.StepCosts(current.Node.State, action, successorState);
                var parent = useParentLinks ? current.Node : null;
                var successor = new Node<TState, TAction>(successorState, parent, action, current.Node.PathCost + stepCost);
                frontier.Add(new FrontierItem(successor, successor.PathCost + _heuristic(successor.State), nextSequence++));
            }

            UpdateQueueMetrics(frontier.Count);
        }

        return null;
    }

    private void ResetMetrics()
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricNodesExpanded, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, 0);
    }

    private void UpdateQueueMetrics(int queueSize)
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, queueSize);
        var maxQueueSize = Metrics.GetInt(QueueSearch<TState, TAction>.MetricMaxQueueSize);
        if (queueSize > maxQueueSize)
        {
            Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, queueSize);
        }
    }

    private sealed class FrontierItem
    {
        public FrontierItem(INode<TState, TAction> node, double priority, long sequence)
        {
            Node = node;
            Priority = priority;
            Sequence = sequence;
        }

        public INode<TState, TAction> Node { get; }

        public double Priority { get; }

        public long Sequence { get; }
    }

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