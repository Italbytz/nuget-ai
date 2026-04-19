using Italbytz.AI.Search.Demos.WeightedGraph;

namespace Italbytz.AI.Tests;

[TestClass]
public class WeightedGraphSearchIntegrationTests
{
    [TestMethod]
    public void Dijkstra_simulation_returns_distances_routes_and_trace()
    {
        var simulator = new WeightedGraphSearchSimulator();
        var result = simulator.SimulateDijkstra(
            "A",
            ["A", "B", "C", "D"],
            [
                new WeightedGraphEdge("A", "B", 4),
                new WeightedGraphEdge("A", "C", 1),
                new WeightedGraphEdge("C", "B", 2),
                new WeightedGraphEdge("B", "D", 1),
                new WeightedGraphEdge("C", "D", 5)
            ]);

        Assert.HasCount(3, result.Routes);
        Assert.HasCount(4, result.Steps);

        var routeToD = result.Routes.Single(route => route.TargetNode == "D");
        CollectionAssert.AreEqual(new[] { "A", "C", "B", "D" }, routeToD.Path.ToArray());
        Assert.AreEqual(4d, routeToD.TotalCost);

        var distanceToB = result.Distances.Single(distance => distance.Node == "B");
        Assert.AreEqual(3d, distanceToB.Distance);
        Assert.AreEqual("C", distanceToB.PreviousNode);

        var firstRelaxations = result.Steps[0].Relaxations;
        Assert.HasCount(2, firstRelaxations);
        Assert.IsTrue(firstRelaxations.All(relaxation => relaxation.Accepted));

        var thirdStep = result.Steps[2];
        Assert.AreEqual("B", thirdStep.ExpandedNode.State);
        Assert.IsTrue(thirdStep.Relaxations.Any(relaxation => relaxation.ToNode == "D" && relaxation.Accepted));
    }
}