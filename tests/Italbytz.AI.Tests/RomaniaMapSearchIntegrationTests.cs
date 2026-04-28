using Italbytz.AI.Search.Agent;
using Italbytz.AI.Search.Demos.Romania;
using Italbytz.AI.Search.Framework.Problem;
using Italbytz.AI.Search.Informed;
using Italbytz.AI.Search.Uninformed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Italbytz.AI.Tests;

[TestClass]
public class RomaniaMapSearchIntegrationTests
{
    [TestMethod]
    public void BreadthFirstSearchPrefersFewestHopsOnRomaniaMap()
    {
        var problem = CreateRomaniaProblem(Sibiu, useDistanceCosts: false);
        var search = new BreadthFirstSearch<string, MoveToAction>();

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray()
            ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { Fagaras, Bucharest }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
    }

    [TestMethod]
    public void UniformCostSearchPrefersCheapestRouteOnRomaniaMap()
    {
        var problem = CreateRomaniaProblem(Sibiu, useDistanceCosts: true);
        var search = new UniformCostSearch<string, MoveToAction>();

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray()
            ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { RimnicuVilcea, Pitesti, Bucharest }, actions);
        Assert.AreEqual(278d, search.Metrics.GetDouble("pathCost"), 0.0001);
        Assert.AreEqual(9, search.Metrics.GetInt("nodesExpanded"));
    }

    [TestMethod]
    public void UniformCostSearchFromAradMatchesLegacyRomaniaSample()
    {
        var problem = CreateRomaniaProblem(Arad, useDistanceCosts: true);
        var search = new UniformCostSearch<string, MoveToAction>();

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray()
            ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { Sibiu, RimnicuVilcea, Pitesti, Bucharest }, actions);
        Assert.AreEqual(418d, search.Metrics.GetDouble("pathCost"), 0.0001);
        Assert.AreEqual(12, search.Metrics.GetInt("nodesExpanded"));
    }

    [TestMethod]
    public void AStarSearchPrefersCheapestRouteOnRomaniaMap()
    {
        var problem = RomaniaMap.CreateProblem(RomaniaMap.Sibiu);
        var search = new AStarSearch<string, RomaniaMoveToAction>(RomaniaMap.HeuristicToBucharest);

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray()
            ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { RomaniaMap.RimnicuVilcea, RomaniaMap.Pitesti, RomaniaMap.Bucharest }, actions);
        Assert.AreEqual(278d, search.Metrics.GetDouble("pathCost"), 0.0001);
        Assert.AreEqual(4, search.Metrics.GetInt("nodesExpanded"));
    }

    [TestMethod]
    public void AStarSearchFromAradMatchesLegacyRomaniaSample()
    {
        var problem = RomaniaMap.CreateProblem(RomaniaMap.Arad);
        var search = new AStarSearch<string, RomaniaMoveToAction>(RomaniaMap.HeuristicToBucharest);

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray()
            ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { RomaniaMap.Sibiu, RomaniaMap.RimnicuVilcea, RomaniaMap.Pitesti, RomaniaMap.Bucharest }, actions);
        Assert.AreEqual(418d, search.Metrics.GetDouble("pathCost"), 0.0001);
        Assert.AreEqual(5, search.Metrics.GetInt("nodesExpanded"));
    }

    [TestMethod]
    public void RomaniaSearchSimulatorExposesAStarFrontierInPriorityOrder()
    {
        var simulator = new RomaniaSearchSimulator();

        var steps = simulator.Simulate(RomaniaMap.Arad, RomaniaSearchAlgorithm.AStarSearch);

        Assert.AreEqual(RomaniaMap.Arad, steps[0].ExpandedNode.State);
        CollectionAssert.AreEqual(
            new[] { RomaniaMap.Sibiu, RomaniaMap.Timisoara, RomaniaMap.Zerind },
            steps[0].Frontier.Select(node => node.State).ToArray());
        Assert.AreEqual(393d, steps[0].Frontier[0].Priority, 0.0001);
        Assert.IsTrue(steps[^1].GoalReached);
        CollectionAssert.AreEqual(
            new[] { RomaniaMap.Arad, RomaniaMap.Sibiu, RomaniaMap.RimnicuVilcea, RomaniaMap.Pitesti, RomaniaMap.Bucharest },
            steps[^1].PathStates.ToArray());
    }

    [TestMethod]
    public void SearchAgentReplaysUniformCostPlanFromArad()
    {
        var problem = CreateRomaniaProblem(Arad, useDistanceCosts: true);
        var search = new UniformCostSearch<string, MoveToAction>();
        var agent = new SearchAgent<object, string, MoveToAction>(problem, search);

        Assert.AreEqual(Sibiu, agent.Act(null)?.ToLocation);
        Assert.AreEqual(RimnicuVilcea, agent.Act(null)?.ToLocation);
        Assert.AreEqual(Pitesti, agent.Act(null)?.ToLocation);
        Assert.AreEqual(Bucharest, agent.Act(null)?.ToLocation);
        Assert.IsNull(agent.Act(null));
        Assert.IsTrue(agent.Done);
    }

    private static GeneralProblem<string, MoveToAction> CreateRomaniaProblem(string initialState, bool useDistanceCosts)
    {
        var roadMap = CreateRoadMap();

        return useDistanceCosts
            ? new GeneralProblem<string, MoveToAction>(
                initialState,
                state => roadMap[state].Select(route => new MoveToAction(route.Next)).ToList(),
                (_, action) => action.ToLocation,
                state => state == Bucharest,
                (state, action, _) => roadMap[state].First(route => route.Next == action.ToLocation).Cost)
            : new GeneralProblem<string, MoveToAction>(
                initialState,
                state => roadMap[state].Select(route => new MoveToAction(route.Next)).ToList(),
                (_, action) => action.ToLocation,
                state => state == Bucharest);
    }

    private static Dictionary<string, List<(string Next, double Cost)>> CreateRoadMap() => new()
    {
        [Arad] = new() { (Zerind, 75), (Sibiu, 140), (Timisoara, 118) },
        [Zerind] = new() { (Arad, 75), (Oradea, 71) },
        [Oradea] = new() { (Zerind, 71), (Sibiu, 151) },
        [Sibiu] = new() { (Arad, 140), (Oradea, 151), (Fagaras, 99), (RimnicuVilcea, 80) },
        [Timisoara] = new() { (Arad, 118), (Lugoj, 111) },
        [Lugoj] = new() { (Timisoara, 111), (Mehadia, 70) },
        [Mehadia] = new() { (Lugoj, 70), (Dobreta, 75) },
        [Dobreta] = new() { (Mehadia, 75), (Craiova, 120) },
        [Craiova] = new() { (Dobreta, 120), (RimnicuVilcea, 146), (Pitesti, 138) },
        [RimnicuVilcea] = new() { (Sibiu, 80), (Craiova, 146), (Pitesti, 97) },
        [Fagaras] = new() { (Sibiu, 99), (Bucharest, 211) },
        [Pitesti] = new() { (RimnicuVilcea, 97), (Craiova, 138), (Bucharest, 101) },
        [Bucharest] = new() { (Fagaras, 211), (Pitesti, 101) }
    };

    private const string Arad = "Arad";
    private const string Zerind = "Zerind";
    private const string Oradea = "Oradea";
    private const string Sibiu = "Sibiu";
    private const string Timisoara = "Timisoara";
    private const string Lugoj = "Lugoj";
    private const string Mehadia = "Mehadia";
    private const string Dobreta = "Dobreta";
    private const string Craiova = "Craiova";
    private const string RimnicuVilcea = "RimnicuVilcea";
    private const string Fagaras = "Fagaras";
    private const string Pitesti = "Pitesti";
    private const string Bucharest = "Bucharest";

    private sealed record MoveToAction(string ToLocation);
}
