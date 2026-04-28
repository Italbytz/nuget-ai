using Italbytz.AI.Agent;
using Italbytz.AI.Search.Framework.Problem;
using Italbytz.AI.Search.Uninformed;

namespace Italbytz.AI.Search.Environment.Map;

public static class MapFunctions
{
    public static GeneralProblem<string, MoveToAction> CreateProblem(
        ExtendableMap map,
        string initialState,
        string goalState,
        bool useDistanceCosts = true)
    {
        return useDistanceCosts
            ? new GeneralProblem<string, MoveToAction>(
                initialState,
                CreateActionsFunction(map),
                CreateResultFunction(),
                state => string.Equals(state, goalState, StringComparison.Ordinal),
                CreateDistanceStepCostFunction(map))
            : new GeneralProblem<string, MoveToAction>(
                initialState,
                CreateActionsFunction(map),
                CreateResultFunction(),
                state => string.Equals(state, goalState, StringComparison.Ordinal));
    }

    public static Func<string, List<MoveToAction>> CreateActionsFunction(ExtendableMap map)
    {
        return state => map.GetPossibleNextLocations(state).Select(location => new MoveToAction(location)).ToList();
    }

    public static Func<string, List<MoveToAction>> CreateReverseActionsFunction(ExtendableMap map)
    {
        return state => map.GetPossiblePreviousLocations(state).Select(location => new MoveToAction(location)).ToList();
    }

    public static Func<string, MoveToAction, string> CreateResultFunction()
    {
        return (_, action) => action.ToLocation;
    }

    public static Func<string, double> CreateHeuristicFunction(
        IReadOnlyDictionary<string, double> estimates,
        double defaultValue = 0)
    {
        return state => estimates.TryGetValue(state, out var estimate) ? estimate : defaultValue;
    }

    public static Func<DynamicPercept, string?> CreatePerceptToStateFunction()
    {
        return percept => percept.GetAttribute(AttNames.PerceptIn) as string;
    }

    public static Func<string, double> CreateSldHeuristicFunction(string goalState, ExtendableMap map)
    {
        return state => GetSld(state, goalState, map);
    }

    public static double GetSld(string from, string to, ExtendableMap map)
    {
        var fromPosition = map.GetPosition(from);
        var toPosition = map.GetPosition(to);
        if (fromPosition is null || toPosition is null)
        {
            return 0;
        }

        var dx = fromPosition.Value.X - toPosition.Value.X;
        var dy = fromPosition.Value.Y - toPosition.Value.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static BidirectionalSearch<string, MoveToAction> CreateBidirectionalSearch(ExtendableMap map, string goalState)
    {
        return new BidirectionalSearch<string, MoveToAction>(goalState, CreateReverseActionsFunction(map), CreateResultFunction());
    }

    public static string GetResult(string _, MoveToAction action)
    {
        return action.ToLocation;
    }

    public static Func<string, MoveToAction, string, double> CreateDistanceStepCostFunction(ExtendableMap map)
    {
        return (state, action, result) =>
            action.ToLocation == result && map.GetDistance(state, action.ToLocation) is double distance
                ? distance
                : 0.1;
    }

    public static bool TestGoal(string state)
    {
        return string.Equals(state, "Bucharest", StringComparison.Ordinal);
    }
}