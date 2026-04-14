// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Adapters.Algorithms.Tests.Environment.Map;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.TwoPly;

public class TwoPlyGameTree
{
    private readonly ExtendableMap aima3eFig5_2;

    public TwoPlyGameTree()
    {
        aima3eFig5_2 = new ExtendableMap();
        aima3eFig5_2.AddUnidirectionalLink("A", "B", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("A", "C", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("A", "D", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("B", "E", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("B", "F", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("B", "G", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("C", "H", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("C", "I", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("C", "J", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("D", "K", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("D", "L", 1.0);
        aima3eFig5_2.AddUnidirectionalLink("D", "M", 1.0);

        Actions = GetActions;
    }

    public Func<TwoPlyGameState, List<MoveToAction>> Actions { get; }

    private List<MoveToAction> GetActions(TwoPlyGameState state)
    {
        var nextPossibleLocations =
            aima3eFig5_2.GetPossibleNextLocations(state.Location);
        return nextPossibleLocations
            .Select(nextLocation => new MoveToAction(nextLocation)).ToList();
    }
}