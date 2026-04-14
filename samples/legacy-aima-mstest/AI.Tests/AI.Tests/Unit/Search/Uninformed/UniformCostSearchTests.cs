using Italbytz.Adapters.Algorithms.AI.Search.Agent;
using Italbytz.Adapters.Algorithms.AI.Search.Framework.Problem;
using Italbytz.Adapters.Algorithms.AI.Search.Framework.QSearch;
using Italbytz.Adapters.Algorithms.AI.Search.Uninformed;
using Italbytz.Adapters.Algorithms.Tests.Environment.Map;
using Italbytz.Ports.Algorithms.AI.Agent;
using Italbytz.Ports.Algorithms.AI.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AI.Tests.Unit.Search.Uniformed;

[TestClass]
public class UniformCostSearchTests
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
        var search = new UniformCostSearch<string, MoveToAction>();
        var actions = TestSimplifiedRoadMapOfRomania(search,
            SimplifiedRoadMapOfPartOfRomania.SIBIU);
        Assert.AreEqual("MoveToAction[name=moveTo, location=RimnicuVilcea], MoveToAction[name=moveTo, location=Pitesti], MoveToAction[name=moveTo, location=Bucharest]", actions);
        Assert.AreEqual("278", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricPathCost));
        Assert.AreEqual("9", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricNodesExpanded));
    }

    [TestMethod]
    public void TestSimplifiedRoadMapOfRomaniaFromArad()
    {
        var search = new UniformCostSearch<string, MoveToAction>();
        var actions = TestSimplifiedRoadMapOfRomania(search,
            SimplifiedRoadMapOfPartOfRomania.ARAD);
        Assert.AreEqual("MoveToAction[name=moveTo, location=Sibiu], MoveToAction[name=moveTo, location=RimnicuVilcea], MoveToAction[name=moveTo, location=Pitesti], MoveToAction[name=moveTo, location=Bucharest]", actions);
        Assert.AreEqual("418", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricPathCost));
        Assert.AreEqual("12", search.Metrics.Get(QueueSearch<string, MoveToAction>.MetricNodesExpanded));
    }

    private static string TestSimplifiedRoadMapOfRomania(
        ISearchForActions<string, MoveToAction> search, string initialState)
    {
        var romaniaMap = new SimplifiedRoadMapOfPartOfRomania();
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