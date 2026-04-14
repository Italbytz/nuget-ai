using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework;

namespace Italbytz.AI.Search.Demos.Romania;

public class RomaniaSearchSimulator
{
    public IReadOnlyList<RomaniaSearchStep> Simulate(string initialState, RomaniaSearchAlgorithm algorithm, string goalState = RomaniaMap.Bucharest)
    {
        var problem = RomaniaMap.CreateProblem(initialState, goalState, useDistanceCosts: algorithm != RomaniaSearchAlgorithm.BreadthFirstSearch);
        var frontier = new List<FrontierItem>();
        var explored = new HashSet<string>();
        var exploredOrder = new List<string>();
        var steps = new List<RomaniaSearchStep>();
        long nextSequence = 0;

        frontier.Add(CreateFrontierItem(new Node<string, RomaniaMoveToAction>(problem.InitialState), algorithm, nextSequence++));

        while (frontier.Count > 0)
        {
            frontier.Sort(FrontierItemComparer.Instance);

            var current = frontier[0];
            frontier.RemoveAt(0);

            if (!explored.Add(current.Node.State))
            {
                continue;
            }

            exploredOrder.Add(current.Node.State);

            if (problem.TestSolution(current.Node))
            {
                steps.Add(CreateStep(steps.Count + 1, current, frontier, exploredOrder, [], goalReached: true));
                break;
            }

            var successors = new List<RomaniaSearchTraceNode>();
            foreach (var action in problem.Actions(current.Node.State))
            {
                var successorState = problem.Result(current.Node.State, action);
                if (explored.Contains(successorState))
                {
                    continue;
                }

                var stepCost = problem.StepCosts(current.Node.State, action, successorState);
                var successor = new Node<string, RomaniaMoveToAction>(successorState, current.Node, action, current.Node.PathCost + stepCost);
                var item = CreateFrontierItem(successor, algorithm, nextSequence++);
                frontier.Add(item);
                successors.Add(ToTraceNode(item));
            }

            steps.Add(CreateStep(steps.Count + 1, current, frontier, exploredOrder, successors, goalReached: false));
        }

        return steps;
    }

    private static FrontierItem CreateFrontierItem(INode<string, RomaniaMoveToAction> node, RomaniaSearchAlgorithm algorithm, long sequence)
    {
        var priority = algorithm switch
        {
            RomaniaSearchAlgorithm.BreadthFirstSearch => node.Depth,
            RomaniaSearchAlgorithm.UniformCostSearch => node.PathCost,
            RomaniaSearchAlgorithm.AStarSearch => node.PathCost + RomaniaMap.HeuristicToBucharest(node.State),
            _ => node.PathCost
        };

        return new FrontierItem(node, priority, sequence);
    }

    private static RomaniaSearchStep CreateStep(
        int stepNumber,
        FrontierItem expandedNode,
        IEnumerable<FrontierItem> frontier,
        IReadOnlyList<string> exploredStates,
        IReadOnlyList<RomaniaSearchTraceNode> successors,
        bool goalReached)
    {
        return new RomaniaSearchStep(
            stepNumber,
            ToTraceNode(expandedNode),
            ToPathStates(expandedNode.Node),
            SearchUtils.ToActions(expandedNode.Node),
            frontier.OrderBy(item => item.Priority).ThenBy(item => item.Sequence).Select(ToTraceNode).ToArray(),
            exploredStates.ToArray(),
            successors.ToArray(),
            goalReached);
    }

    private static IReadOnlyList<string> ToPathStates(INode<string, RomaniaMoveToAction> node)
    {
        var stack = new Stack<string>();
        var current = node;
        while (current is not null)
        {
            stack.Push(current.State);
            current = current.Parent;
        }

        return stack.ToArray();
    }

    private static RomaniaSearchTraceNode ToTraceNode(FrontierItem item)
    {
        return new RomaniaSearchTraceNode(item.Node.State, item.Node.PathCost, item.Priority, item.Node.Depth);
    }

    private sealed record FrontierItem(INode<string, RomaniaMoveToAction> Node, double Priority, long Sequence);

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