using Italbytz.Adapters.Algorithms.AI.Search.CSP;
using Italbytz.Adapters.Algorithms.AI.Search.CSP.Examples;
using Italbytz.Adapters.Algorithms.AI.Search.CSP.Solver;
using Italbytz.Ports.Algorithms.AI.Search.CSP;

namespace AI.Tests.Unit.Search.CSP;

[TestClass]
public class MapCspTests
{
    private ICSP<Variable, string> csp;
    
    [TestInitialize]
    public void TestInitialize()
    {
        csp = new MapCSP();
    }
    
    [TestMethod]
    public void TestStandardBackTrackingSearch()
    {
        var solver = new FlexibleBacktrackingSolver<Variable, string>();
        var assignment = solver.Solve(csp);
        Assert.IsNotNull(assignment);
            Assert.AreEqual(MapCSP.BLUE, assignment.GetValue(MapCSP.WA));
            Assert.AreEqual(MapCSP.RED, assignment.GetValue(MapCSP.NT));
            Assert.AreEqual(MapCSP.GREEN, assignment.GetValue(MapCSP.SA));
            Assert.AreEqual(MapCSP.BLUE, assignment.GetValue(MapCSP.Q));
            Assert.AreEqual(MapCSP.RED, assignment.GetValue(MapCSP.NSW));
            Assert.AreEqual(MapCSP.BLUE, assignment.GetValue(MapCSP.V));
            Assert.AreEqual(MapCSP.RED, assignment.GetValue(MapCSP.T));
    }   
    
    [TestMethod]
    public void TestRecommendedBackTrackingSearch()
    {
        var solver = new FlexibleBacktrackingSolver<Variable, string>
            {
                variableSelectionStrategy = new CspHeuristics.MrvDegHeuristic<Variable,string>(),
                valueOrderingStrategy = new CspHeuristics.LeastConstrainingValueHeuristic<Variable,string>()
            };

        var assignment = solver.Solve(csp);
        Assert.IsNotNull(assignment);
    }   
}