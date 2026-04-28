using Italbytz.AI.Search;
using Italbytz.AI.Search.Demos.Romania;
using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;
using Italbytz.AI.Search.Informed;
using Italbytz.AI.Search.Uninformed;

namespace Italbytz.AI.Tests;

[TestClass]
public class InformedSearchLegacyRegressionTests
{
    [TestMethod]
    public void GreedyBestFirstSearchMatchesLegacyRomaniaRouteFromArad()
    {
        var problem = RomaniaMap.CreateProblem(RomaniaMap.Arad);
        var search = new GreedyBestFirstSearch<string, RomaniaMoveToAction>(node => RomaniaMap.HeuristicToBucharest(node.State));

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { RomaniaMap.Sibiu, RomaniaMap.Fagaras, RomaniaMap.Bucharest }, actions);
        Assert.AreEqual(450d, search.Metrics.GetDouble(QueueSearch<string, RomaniaMoveToAction>.MetricPathCost), 0.0001);
        Assert.AreEqual(3, search.Metrics.GetInt(QueueSearch<string, RomaniaMoveToAction>.MetricNodesExpanded));
    }

    [TestMethod]
    public void RecursiveBestFirstSearchMatchesLegacyRomaniaRouteFromArad()
    {
        var problem = RomaniaMap.CreateProblem(RomaniaMap.Arad);
        var search = new RecursiveBestFirstSearch<string, RomaniaMoveToAction>(node => RomaniaMap.HeuristicToBucharest(node.State), avoidLoops: true);

        var actions = search.FindActions(problem)?.Select(action => action.ToLocation).ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { RomaniaMap.Sibiu, RomaniaMap.RimnicuVilcea, RomaniaMap.Pitesti, RomaniaMap.Bucharest }, actions);
        Assert.AreEqual(418d, search.Metrics.GetDouble(RecursiveBestFirstSearch<string, RomaniaMoveToAction>.MetricPathCost), 0.0001);
        Assert.AreEqual(6, search.Metrics.GetInt(RecursiveBestFirstSearch<string, RomaniaMoveToAction>.MetricNodesExpanded));
    }
}