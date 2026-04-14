namespace Italbytz.AI.Search.Demos.Romania;

public enum RomaniaSearchAlgorithm
{
    BreadthFirstSearch,
    UniformCostSearch,
    AStarSearch
}

public sealed record RomaniaMoveToAction(string ToLocation);

public sealed record RomaniaRoad(string From, string To, double Cost);

public sealed record RomaniaSearchTraceNode(string State, double PathCost, double Priority, int Depth);

public sealed record RomaniaSearchStep(
    int StepNumber,
    RomaniaSearchTraceNode ExpandedNode,
    IReadOnlyList<string> PathStates,
    IReadOnlyList<RomaniaMoveToAction> PathActions,
    IReadOnlyList<RomaniaSearchTraceNode> Frontier,
    IReadOnlyList<string> ExploredStates,
    IReadOnlyList<RomaniaSearchTraceNode> Successors,
    bool GoalReached);