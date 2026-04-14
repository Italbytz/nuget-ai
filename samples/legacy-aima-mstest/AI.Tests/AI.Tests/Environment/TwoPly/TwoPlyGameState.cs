// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

namespace Italbytz.Adapters.Algorithms.Tests.Environment.TwoPly;

public class TwoPlyGameState
{
    public TwoPlyGameState(string location) => Location = location;

    public string Location { get; }

    public override bool Equals(object? obj)
    {
        if (obj is TwoPlyGameState state)
            return Location.Equals(state.Location);

        return base.Equals(obj);
    }

    public override int GetHashCode() => Location.GetHashCode();

    public override string ToString() => Location;
}