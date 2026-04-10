using Italbytz.AI.Planning;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Tests;

[TestClass]
public class PlanningIntegrationTests
{
    [TestMethod]
    public void Utils_parse_reads_positive_and_negative_literals()
    {
        var literals = Utils.Parse("At(C1,JFK) ^ ~At(C2,SFO)");

        Assert.HasCount(2, literals);
        Assert.AreEqual("At(C1,JFK)", literals[0].ToString());
        Assert.AreEqual("~At(C2,SFO)", literals[1].ToString());
    }

    [TestMethod]
    public void Planning_problem_propositionalises_actions_for_known_constants()
    {
        var initialState = new State("At(C1,JFK)");
        var goal = Utils.Parse("At(C1,SFO)");
        var fly = new ActionSchema(
            "Fly",
            [new Variable("p")],
            "At(p,JFK)",
            "~At(p,JFK)^At(p,SFO)");

        var problem = new PlanningProblem(initialState, goal, fly);
        var actions = problem.GetPropositionalisedActions().ToList();

        Assert.HasCount(3, actions);
        Assert.IsTrue(actions.All(action => action.Name == "Fly"));
        Assert.IsTrue(actions.Any(action => action.Precondition[0].ToString() == "At(C1,JFK)"));
    }

    [TestMethod]
    public void Hierarchical_search_finds_the_direct_taxi_plan()
    {
        var result = new HierarchicalSearchAlgorithm().HierarchicalSearch(
            PlanningProblemFactory.GoHomeToSfoProblem());

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Taxi", result.First().Name);
    }
}
