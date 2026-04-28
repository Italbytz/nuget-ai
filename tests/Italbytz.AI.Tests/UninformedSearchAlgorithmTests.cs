using Italbytz.AI.Search.Framework.Problem;
using Italbytz.AI.Search.Uninformed;

namespace Italbytz.AI.Tests;

[TestClass]
public class UninformedSearchAlgorithmTests
{
    [TestMethod]
    public void DepthFirstSearch_FindsDeepGoalWithoutLooping()
    {
        var problem = CreateLoopingProblem();
        var search = new DepthFirstSearch<string, string>();

        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "to-left", "left-goal" }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
    }

    [TestMethod]
    public void DepthLimitedSearch_ReportsCutoffWhenLimitTooSmall()
    {
        var problem = CreateLoopingProblem();
        var search = new DepthLimitedSearch<string, string>(1);

        var node = search.FindNode(problem);

        Assert.IsNull(node);
        Assert.IsTrue(search.IsCutoffResult(node));
    }

    [TestMethod]
    public void DepthLimitedSearch_FindsGoalAtExactLimit()
    {
        var problem = CreateLoopingProblem();
        var search = new DepthLimitedSearch<string, string>(2);

        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "to-left", "left-goal" }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
    }

    [TestMethod]
    public void IterativeDeepeningSearch_FindsGoalAcrossIncreasingLimits()
    {
        var problem = CreateLoopingProblem();
        var search = new IterativeDeepeningSearch<string, string>();

        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "to-left", "left-goal" }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
        Assert.AreNotEqual(0, search.Metrics.GetInt("nodesExpanded"));
    }

    private static GeneralProblem<string, string> CreateLoopingProblem()
    {
        var transitions = new Dictionary<string, List<(string Action, string Next)>>
        {
            ["start"] = new() { ("to-left", "left"), ("to-right", "right") },
            ["left"] = new() { ("left-loop", "start"), ("left-goal", "goal") },
            ["right"] = new() { ("right-dead", "dead") },
            ["dead"] = new(),
            ["goal"] = new()
        };

        return new GeneralProblem<string, string>(
            "start",
            state => transitions[state].Select(t => t.Action).ToList(),
            (state, action) => transitions[state].First(t => t.Action == action).Next,
            state => state == "goal");
    }
}