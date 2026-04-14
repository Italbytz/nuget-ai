using Italbytz.Adapters.Algorithms.AI.Logic.Planning;

namespace AI.Tests.Unit.Logic.Planning;

[TestClass]
public class HierarchicalSearchTest
{
    [TestMethod]
    public void TestHierarchicalSearch()
    {
        var algo = new HierarchicalSearchAlgorithm();
        var taxiAction = new ActionSchema("Taxi", null,
            "At(Home)",
            "~At(Home)^At(SFO)");
        var result = algo.HierarchicalSearch(
            PlanningProblemFactory.GoHomeToSfoProblem());
        Assert.IsNotNull(result);
        foreach (var action in result)
            Assert.AreEqual(action, taxiAction);
    }
}