// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Adapters.Algorithms.AI.Agent;
using Italbytz.Adapters.Algorithms.AI.Util.Datastructure;
using Italbytz.Ports.Algorithms.AI.Agent;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.NQueens;

public class NQueensEnvironment : AbstractEnvironment<IPercept, QueenAction>
{
    public NQueensEnvironment(NQueensBoard board) => Board = board;

    public NQueensBoard Board { get; }

    protected override void Execute(IAgent<IPercept, QueenAction> agent,
        QueenAction? action)
    {
        var loc = new XYLocation(action.X, action.Y);
        switch (action.Name)
        {
            case QueenAction.PLACE_QUEEN:
                Board.AddQueenAt(loc);
                break;
            case QueenAction.REMOVE_QUEEN:
                Board.RemoveQueenFrom(loc);
                break;
            case QueenAction.MOVE_QUEEN:
                Board.MoveQueenTo(loc);
                break;
        }
    }

    protected override IPercept? GetPerceptSeenBy(
        IAgent<IPercept, QueenAction> agent) =>
        null;
}