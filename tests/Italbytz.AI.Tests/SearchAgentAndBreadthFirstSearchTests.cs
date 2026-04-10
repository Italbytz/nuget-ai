using Italbytz.AI.Search.Agent;
using Italbytz.AI.Search.Framework.Problem;
using Italbytz.AI.Search.Uninformed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Italbytz.AI.Tests;

[TestClass]
public class SearchAgentAndBreadthFirstSearchTests
{
    [TestMethod]
    public void BreadthFirstSearchReturnsShortestActionSequence()
    {
        var transitions = new Dictionary<string, Dictionary<string, string>>
        {
            ["start"] = new()
            {
                ["to-b"] = "b",
                ["to-c"] = "c"
            },
            ["b"] = new()
            {
                ["to-goal"] = "goal"
            },
            ["c"] = new()
            {
                ["to-d"] = "d"
            },
            ["d"] = new()
            {
                ["to-goal-from-d"] = "goal"
            },
            ["goal"] = new()
        };

        var problem = new GeneralProblem<string, string>(
            "start",
            state => transitions[state].Keys.ToList(),
            (state, action) => transitions[state][action],
            state => state == "goal");

        var search = new BreadthFirstSearch<string, string>();
        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "to-b", "to-goal" }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
    }

    [TestMethod]
    public void SearchAgentDequeuesPlannedActionsUntilDone()
    {
        var problem = new GeneralProblem<int, string>(
            0,
            state => state == 0
                ? new List<string> { "advance", "skip" }
                : state == 1
                    ? new List<string> { "finish" }
                    : new List<string>(),
            (state, action) => (state, action) switch
            {
                (0, "advance") => 1,
                (0, "skip") => 2,
                (1, "finish") => 3,
                _ => state
            },
            state => state == 3);

        var search = new BreadthFirstSearch<int, string>();
        var agent = new SearchAgent<object, int, string>(problem, search);

        Assert.AreEqual("advance", agent.Act(null));
        Assert.AreEqual("finish", agent.Act(null));
        Assert.IsNull(agent.Act(null));
        Assert.IsTrue(agent.Done);
    }

    [TestMethod]
    public void UniformCostSearchPrefersCheapestPathInGraphSearchMode()
    {
        var transitions = new Dictionary<string, List<(string Action, string Next, double Cost)>>
        {
            ["start"] = new() { ("fast", "goal", 5.0), ("cheap", "mid", 1.0) },
            ["mid"] = new() { ("finish", "goal", 1.0), ("back", "start", 1.0) },
            ["goal"] = new()
        };

        var problem = new GeneralProblem<string, string>(
            "start",
            state => transitions[state].Select(x => x.Action).ToList(),
            (state, action) => transitions[state].First(x => x.Action == action).Next,
            state => state == "goal",
            (state, action, _) => transitions[state].First(x => x.Action == action).Cost);

        var search = new UniformCostSearch<string, string>();
        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "cheap", "finish" }, actions);
        Assert.AreEqual(2d, search.Metrics.GetDouble("pathCost"), 0.0001);
    }
}
