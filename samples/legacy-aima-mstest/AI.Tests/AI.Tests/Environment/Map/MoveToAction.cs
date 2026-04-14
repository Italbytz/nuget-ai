// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Adapters.Algorithms.AI.Agent;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.Map;

public class MoveToAction : DynamicAction
{
    private const string AttributeMoveToLocation = "location";

    public MoveToAction(string location) : base("moveTo") =>
        Attributes[AttributeMoveToLocation] = location;

    public string ToLocation => (string)Attributes[AttributeMoveToLocation];
}