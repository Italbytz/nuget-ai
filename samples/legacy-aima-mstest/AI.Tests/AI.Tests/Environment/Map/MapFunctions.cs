// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Ports.Algorithms.AI.Search;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.Map;

public static class MapFunctions
{
    public static Func<string, List<MoveToAction>> CreateActionsFunction(
        IMap map)
    {
        return state => map.GetPossibleNextLocations(state)
            .Select(loc => new MoveToAction(loc)).ToList();
    }

    public static bool TestGoal(string arg) =>
        arg.Equals(SimplifiedRoadMapOfPartOfRomania.BUCHAREST);

    public static Func<string, MoveToAction, string, double>
        CreateDistanceStepCostFunction(IMap map)
    {
        return (state, action, statePrimed) =>
        {
            var distance = map.GetDistance(state, statePrimed);
            // Used by Uniform-cost search to ensure every step is greater than or equal
            // to some small positive constant
            return distance != null && distance > 0 ? distance : 0.1;
        };
    }

    public static string GetResult(string state, MoveToAction action) =>
        action.ToLocation;

    public static Func<INode<string, MoveToAction>, double>
        CreateSLDHeuristicFunction(string goal, IMap map)
    {
        return node => GetSLD(node.State, goal, map);
    }

    private static double GetSLD(string loc1, string loc2, IMap map)
    {
        var result = 0.0;
        var pt1 = map.GetPosition(loc1);
        var pt2 = map.GetPosition(loc2);
        if (pt1 != null && pt2 != null)
            result = pt1.Distance(pt2);
        return result;
    }
}