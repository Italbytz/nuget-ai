// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using Italbytz.Adapters.Algorithms.AI.Search.Agent;
using Italbytz.Adapters.Algorithms.AI.Search.Framework.Problem;
using Italbytz.Adapters.Algorithms.AI.Search.Framework.QSearch;
using Italbytz.Adapters.Algorithms.AI.Search.Informed;
using Italbytz.Adapters.Algorithms.Tests.Environment.Map;
using Italbytz.Ports.Algorithms.AI.Agent;
using Italbytz.Ports.Algorithms.AI.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AI.Tests.Unit.Search.Informed;

[TestClass]
public sealed class AStarSearchTests
{
    private const bool ConsoleLogging = true;

    private static ILoggerFactory _loggerFactory =
        NullLoggerFactory.Instance;
    
    [TestInitialize]
    public void TestInitialize()
    {
        if (ConsoleLogging)
            _loggerFactory =
                LoggerFactory.Create(builder => builder.AddConsole());
    }
    
    [TestCleanup]
    public void TestCleanup()
    {
        _loggerFactory.Dispose();
    }
    
    [TestMethod]
    public void TestSimplifiedRoadMapOfRomaniaFromSibiu()
    {
        var romaniaMap = new SimplifiedRoadMapOfPartOfRomania();
        var search = new AStarSearch<string, MoveToAction>(
            new GraphSearch<string, MoveToAction>(),
            MapFunctions.CreateSLDHeuristicFunction(
                SimplifiedRoadMapOfPartOfRomania.BUCHAREST, romaniaMap));
        var actions = TestSimplifiedRoadMapOfRomania(search, romaniaMap,
            SimplifiedRoadMapOfPartOfRomania.SIBIU);
        
        Assert.AreEqual("MoveToAction[name=moveTo, location=RimnicuVilcea], MoveToAction[name=moveTo, location=Pitesti], MoveToAction[name=moveTo, location=Bucharest]", actions);
        Assert.AreEqual("278", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricPathCost));
        Assert.AreEqual("4", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricNodesExpanded));
    }
    
    [TestMethod]
    public void TestSimplifiedRoadMapOfRomaniaFromArad()
    {
        var romaniaMap = new SimplifiedRoadMapOfPartOfRomania();
        var search = new AStarSearch<string, MoveToAction>(
            new GraphSearch<string, MoveToAction>(),
            MapFunctions.CreateSLDHeuristicFunction(
                SimplifiedRoadMapOfPartOfRomania.BUCHAREST, romaniaMap));
        var actions = TestSimplifiedRoadMapOfRomania(search, romaniaMap,
            SimplifiedRoadMapOfPartOfRomania.ARAD);
        Assert.AreEqual("MoveToAction[name=moveTo, location=Sibiu], MoveToAction[name=moveTo, location=RimnicuVilcea], MoveToAction[name=moveTo, location=Pitesti], MoveToAction[name=moveTo, location=Bucharest]", actions);
        Assert.AreEqual("418", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricPathCost));
        Assert.AreEqual("5", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricNodesExpanded));
    }
    
    private static string TestSimplifiedRoadMapOfRomania(
        ISearchForActions<string, MoveToAction> search, IMap romaniaMap,
        string initialState)
    {
        var problem = new GeneralProblem<string, MoveToAction>(initialState,
            MapFunctions.CreateActionsFunction(romaniaMap),
            MapFunctions.GetResult, MapFunctions.TestGoal,
            MapFunctions.CreateDistanceStepCostFunction(romaniaMap));
        var agent = new SearchAgent<IPercept, string, MoveToAction>(problem,
            search, _loggerFactory);
        var actions = agent.Actions;
        return string.Join(", ", actions);
    }
}