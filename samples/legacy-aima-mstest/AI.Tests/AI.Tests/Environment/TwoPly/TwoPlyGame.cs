// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Adapters.Algorithms.Tests.Environment.Map;
using Italbytz.Ports.Algorithms.AI.Search.Adversarial;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.TwoPly;

public class TwoPlyGame : IGame<TwoPlyGameState, MoveToAction, string>
{
    public TwoPlyGameState InitialState { get; } = new("A");
    public string[] Players { get; } = { "MAX", "MIN" };

    public Func<TwoPlyGameState, string> Player { get; } = state =>
    {
        switch (state.Location)
        {
            case "B":
            case "C":
            case "D":
                return "MIN";
            default:
                return "MAX";
        }
    };

    public Func<TwoPlyGameState, List<MoveToAction>> Actions { get; } =
        state => new TwoPlyGameTree().Actions(state);

    public Func<TwoPlyGameState, MoveToAction, TwoPlyGameState>
        Result { get; } = (_, action) => new TwoPlyGameState(action.ToLocation);

    public Func<TwoPlyGameState, bool> Terminal { get; } =
        state => state.Location.ToCharArray()[0] > 'D';

    public Func<TwoPlyGameState, string, double> Utility { get; } =
        (state, player) =>
        {
            return state.Location switch
            {
                // B
                "E" => 3,
                "F" => 12,
                "G" => 8,
                // C
                "H" => 2,
                "I" => 4,
                "J" => 6,
                // D
                "K" => 14,
                "L" => 5,
                "M" => 2,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(state.Location), state.Location, null)
            };
        };
}