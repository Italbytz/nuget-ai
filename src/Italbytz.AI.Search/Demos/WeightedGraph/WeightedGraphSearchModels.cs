namespace Italbytz.AI.Search.Demos.WeightedGraph;

public sealed record WeightedGraphEdge(string FromNode, string ToNode, double Cost);

public sealed record WeightedGraphMoveToAction(string ToNode, double Cost);

public sealed record WeightedGraphTraceNode(string State, double PathCost, double Priority, int Depth);

public sealed record WeightedGraphDistanceState(string Node, double? Distance, string? PreviousNode);

public sealed record WeightedGraphRelaxation(string FromNode, string ToNode, double CandidateDistance, bool Accepted);

public sealed record WeightedGraphRoute(string TargetNode, IReadOnlyList<string> Path, double TotalCost);

public sealed record WeightedGraphSearchStep(
    int StepNumber,
    WeightedGraphTraceNode ExpandedNode,
    IReadOnlyList<WeightedGraphTraceNode> Frontier,
    IReadOnlyList<WeightedGraphDistanceState> Distances,
    IReadOnlyList<WeightedGraphRelaxation> Relaxations);

public sealed record WeightedGraphSearchResult(
    IReadOnlyList<WeightedGraphDistanceState> Distances,
    IReadOnlyList<WeightedGraphRoute> Routes,
    IReadOnlyList<WeightedGraphSearchStep> Steps);