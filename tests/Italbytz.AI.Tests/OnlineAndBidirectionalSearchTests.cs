using Italbytz.AI.Search.Framework.Problem;
using Italbytz.AI.Search.Online;
using Italbytz.AI.Search.Uninformed;

namespace Italbytz.AI.Tests;

[TestClass]
public class OnlineAndBidirectionalSearchTests
{
    [TestMethod]
    public void BidirectionalSearch_FindsShortestPathOnUndirectedGraph()
    {
        var transitions = CreateUndirectedMap();
        var problem = CreateProblem("A", "G", transitions);
        var search = new BidirectionalSearch<string, string>("G", s => transitions[s].Select(t => t.Action).ToList(), (s, a) => transitions[s].First(t => t.Action == a).Next);

        var actions = search.FindActions(problem)?.ToArray() ?? Array.Empty<string>();

        CollectionAssert.AreEqual(new[] { "A-B", "B-D", "D-G" }, actions);
        Assert.AreEqual("G", search.FindState(problem));
    }

    [TestMethod]
    public void OnlineDfsAgent_BacktracksUntilGoalReached()
    {
        var transitions = CreateUndirectedMap();
        var problem = CreateOnlineProblem("G", transitions);
        var agent = new OnlineDFSAgent<string, string, string>(problem, percept => percept ?? string.Empty);

        var current = "A";
        var visitedActions = new List<string>();
        while (agent.Alive)
        {
            var action = agent.Act(current);
            if (action is null) break;
            visitedActions.Add(action);
            current = transitions[current].First(t => t.Action == action).Next;
        }

        CollectionAssert.AreEqual(new[] { "A-B", "B-A", "A-C", "C-A", "A-C", "C-A", "A-B", "B-D", "D-B", "B-E", "E-B", "B-E", "E-B", "B-D", "D-F", "F-D", "D-G" }, visitedActions);
        Assert.AreEqual("G", current);
    }

    [TestMethod]
    public void LrtaStarAgent_UsesHeuristicAndEventuallyReachesGoal()
    {
        var transitions = new Dictionary<string, List<(string Action, string Next, double Cost)>>
        {
            ["A"] = new() { ("A-B", "B", 4) },
            ["B"] = new() { ("B-A", "A", 4), ("B-C", "C", 4) },
            ["C"] = new() { ("C-B", "B", 4), ("C-D", "D", 4) },
            ["D"] = new() { ("D-C", "C", 4), ("D-E", "E", 4) },
            ["E"] = new() { ("E-D", "D", 4), ("E-F", "F", 4) },
            ["F"] = new()
        };

        var problem = new GeneralProblem<string, string>(
            "A",
            state => transitions[state].Select(t => t.Action).ToList(),
            (state, action) => transitions[state].First(t => t.Action == action).Next,
            state => state == "F",
            (state, action, next) => transitions[state].First(t => t.Action == action && t.Next == next).Cost);

        var agent = new LRTAStarAgent<string, string, string>(problem, percept => percept ?? string.Empty, _ => 1.0);
        var current = "A";
        var visitedActions = new List<string>();
        while (agent.Alive && visitedActions.Count < 20)
        {
            var action = agent.Act(current);
            if (action is null) break;
            visitedActions.Add(action);
            current = transitions[current].First(t => t.Action == action).Next;
        }

        CollectionAssert.AreEqual(new[] { "A-B", "B-A", "A-B", "B-C", "C-B", "B-C", "C-D", "D-C", "C-D", "D-E", "E-D", "D-E", "E-F" }, visitedActions);
        Assert.AreEqual("F", current);
    }

    private static GeneralProblem<string, string> CreateProblem(string initialState, string goalState, Dictionary<string, List<(string Action, string Next)>> transitions)
    {
        return new GeneralProblem<string, string>(
            initialState,
            state => transitions[state].Select(t => t.Action).ToList(),
            (state, action) => transitions[state].First(t => t.Action == action).Next,
            state => state == goalState);
    }

    private static GeneralProblem<string, string> CreateOnlineProblem(string goalState, Dictionary<string, List<(string Action, string Next)>> transitions)
    {
        return new GeneralProblem<string, string>(
            string.Empty,
            state => transitions[state].Select(t => t.Action).ToList(),
            (state, action) => transitions[state].First(t => t.Action == action).Next,
            state => state == goalState);
    }

    private static Dictionary<string, List<(string Action, string Next)>> CreateUndirectedMap() => new()
    {
        ["A"] = new() { ("A-B", "B"), ("A-C", "C") },
        ["B"] = new() { ("B-A", "A"), ("B-D", "D"), ("B-E", "E") },
        ["C"] = new() { ("C-A", "A") },
        ["D"] = new() { ("D-B", "B"), ("D-F", "F"), ("D-G", "G") },
        ["E"] = new() { ("E-B", "B") },
        ["F"] = new() { ("F-D", "D") },
        ["G"] = new() { ("G-D", "D") }
    };
}