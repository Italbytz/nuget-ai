using Italbytz.AI.Agent;
using Italbytz.AI.Search.Environment.Map;
using Italbytz.AI.Search.Informed;
using Italbytz.AI.Search.Uninformed;

namespace Italbytz.AI.Tests;

[TestClass]
public class MapEnvironmentTests
{
    [TestMethod]
    public void ExtendableMap_ReturnsLinkedLocationsAndDistances()
    {
        var map = CreateMap();

        var linkedToB = map.GetPossibleNextLocations("B");

        CollectionAssert.AreEquivalent(new[] { "A", "C", "E" }, linkedToB);
        Assert.AreEqual(5.0, map.GetDistance("A", "B"));
        Assert.AreEqual(14.0, map.GetDistance("B", "E"));
        Assert.IsNull(map.GetDistance("E", "B"));
    }

    [TestMethod]
    public void ExtendableMap_ReturnsPreviousLocationsForReverseTraversal()
    {
        var map = CreateMap();

        var previousToC = map.GetPossiblePreviousLocations("C");
        var previousToE = map.GetPossiblePreviousLocations("E");

        CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, previousToC);
        CollectionAssert.AreEquivalent(new[] { "B" }, previousToE);
    }

    [TestMethod]
    public void ExtendableMap_RandomDestinationIsAlwaysKnownLocation()
    {
        var map = CreateMap();
        var knownLocations = new HashSet<string>(new[] { "A", "B", "C", "D", "E" });

        for (var i = 0; i < knownLocations.Count; i++)
        {
            Assert.Contains(map.RandomlyGenerateDestination(), knownLocations);
        }
    }

    [TestMethod]
    public void MapFunctions_CreateActionsResultAndCosts()
    {
        var map = CreateMap();
        var actionsFn = MapFunctions.CreateActionsFunction(map);
        var resultFn = MapFunctions.CreateResultFunction();
        var stepCostFn = MapFunctions.CreateDistanceStepCostFunction(map);

        var actions = actionsFn("A");

        CollectionAssert.AreEquivalent(new[] { "B", "C" }, actions.Select(a => a.ToLocation).ToArray());
        Assert.AreEqual("B", resultFn("A", actions.First(a => a.ToLocation == "B")));
        Assert.AreEqual(5.0, stepCostFn("A", new MoveToAction("B"), "B"), 0.001);
        Assert.AreEqual(0.1, stepCostFn("A", new MoveToAction("E"), "E"), 0.001);
    }

    [TestMethod]
    public void MapFunctions_CreateReverseActionsBidirectionalSearchAndHeuristic()
    {
        var map = CreateMap();
        map.SetPosition("A", 0, 0);
        map.SetPosition("C", 6, 0);
        map.SetPosition("D", 13, 0);
        map.SetPosition("B", 0, 5);
        map.SetPosition("E", 5, 7);
        var reverseActionsFn = MapFunctions.CreateReverseActionsFunction(map);
        var heuristicFn = MapFunctions.CreateHeuristicFunction(new Dictionary<string, double>
        {
            ["A"] = 6,
            ["B"] = 5,
            ["C"] = 2,
            ["D"] = 0,
            ["E"] = 7
        });
        var bidirectionalSearch = MapFunctions.CreateBidirectionalSearch(map, "D");
        var problem = MapFunctions.CreateProblem(map, "A", "D");
        var aStar = new AStarSearch<string, MoveToAction>(heuristicFn);

        CollectionAssert.AreEquivalent(new[] { "C" }, reverseActionsFn("D").Select(action => action.ToLocation).ToArray());
        CollectionAssert.AreEqual(new[] { "C", "D" }, bidirectionalSearch.FindActions(problem)?.Select(action => action.ToLocation).ToArray());
        CollectionAssert.AreEqual(new[] { "C", "D" }, aStar.FindActions(problem)?.Select(action => action.ToLocation).ToArray());
        Assert.AreEqual(13d, MapFunctions.GetSld("A", "D", map), 0.001);
        Assert.AreEqual(7d, MapFunctions.CreateSldHeuristicFunction("D", map)("C"), 0.001);
    }

    [TestMethod]
    public void MapEnvironment_TracksMultipleAgentsAndPercepts()
    {
        var environment = new MapEnvironment(CreateMap());
        var agentA = new PassiveMapAgent();
        var agentB = new PassiveMapAgent();

        environment.AddAgent(agentA, "A");
        environment.AddAgent(agentB, "D");
        environment.Execute(agentA, new MoveToAction("B"));
        environment.Execute(agentB, new MoveToAction("C"));

        Assert.AreEqual("B", environment.GetAgentLocation(agentA));
        Assert.AreEqual("C", environment.GetAgentLocation(agentB));
        Assert.AreEqual("B", environment.GetPerceptSeenBy(agentA)?.GetAttribute(AttNames.PerceptIn));
    }

    [TestMethod]
    public void SimpleMapAgent_UsesSearchToReachConfiguredGoal()
    {
        var map = CreateMap();
        var environment = new MapEnvironment(map);
        var agent = new SimpleMapAgent(map, new UniformCostSearch<string, MoveToAction>(), "D");
        var executedLocations = new List<string>();

        environment.AddAgent(agent, "A");

        while (true)
        {
            var action = agent.Act(environment.GetPerceptSeenBy(agent));
            if (action is null)
            {
                break;
            }

            executedLocations.Add(action.ToLocation);
            environment.Execute(agent, action);
        }

        CollectionAssert.AreEqual(new[] { "C", "D" }, executedLocations);
        Assert.AreEqual("D", environment.GetAgentLocation(agent));
        Assert.IsGreaterThan(0, agent.SearchMetrics.GetInt("nodesExpanded"));
    }

    [TestMethod]
    public void SimpleMapAgent_NotifierReportsGoalAndSearch()
    {
        var map = CreateMap();
        var environment = new MapEnvironment(map);
        var notifications = new List<string>();
        var agent = new SimpleMapAgent(map, new UniformCostSearch<string, MoveToAction>(), "D")
            .SetNotifier(message => notifications.Add(message));

        environment.AddAgent(agent, "A");

        var action = agent.Act(environment.GetPerceptSeenBy(agent));

        Assert.IsNotNull(action);
        Assert.HasCount(2, notifications);
        StringAssert.Contains(notifications[0], "CurrentLocation=In(A)");
        StringAssert.Contains(notifications[0], "Goal=In(D)");
        StringAssert.Contains(notifications[1], "Search");
    }

    [TestMethod]
    public void SimpleMapAgent_ProcessesMultipleGoalsSequentially()
    {
        var map = CreateMap();
        var environment = new MapEnvironment(map);
        var agent = new SimpleMapAgent(
            map,
            new UniformCostSearch<string, MoveToAction>(),
            new[] { "D", "E" });
        var executedLocations = new List<string>();

        environment.AddAgent(agent, "A");

        while (true)
        {
            var action = agent.Act(environment.GetPerceptSeenBy(agent));
            if (action is null)
            {
                break;
            }

            executedLocations.Add(action.ToLocation);
            environment.Execute(agent, action);
        }

        CollectionAssert.AreEqual(new[] { "C", "D", "C", "B", "E" }, executedLocations);
        Assert.AreEqual("E", environment.GetAgentLocation(agent));
    }

    private static ExtendableMap CreateMap()
    {
        var map = new ExtendableMap();
        map.AddBidirectionalLink("A", "B", 5.0);
        map.AddBidirectionalLink("A", "C", 6.0);
        map.AddBidirectionalLink("B", "C", 4.0);
        map.AddBidirectionalLink("C", "D", 7.0);
        map.AddUnidirectionalLink("B", "E", 14.0);
        return map;
    }

    private sealed class PassiveMapAgent : SimpleAgent<DynamicPercept, MoveToAction>
    {
    }
}