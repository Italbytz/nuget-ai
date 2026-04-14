using Italbytz.Adapters.Algorithms.AI.Search.CSP;
using Italbytz.Adapters.Algorithms.AI.Search.CSP.Examples;
using Italbytz.Adapters.Algorithms.AI.Search.CSP.Solver;
using Italbytz.Ports.Algorithms.AI.Search.CSP;

namespace AI.Tests.Unit.Search.CSP;

[TestClass]
public class TreeCspSolverTests
{
    private static readonly IVariable WA = new Variable("WA");
    private static readonly IVariable NT = new Variable("NT");
    private static readonly IVariable Q = new Variable("Q");
    private static readonly IVariable NSW = new Variable("NSW");
    private static readonly IVariable V = new Variable("V");

    private static readonly IConstraint<IVariable, string> C1 =
        new NotEqualConstraint<IVariable, string>(WA, NT);

    private static readonly IConstraint<IVariable, string> C2 =
        new NotEqualConstraint<IVariable, string>(NT, Q);

    private static readonly IConstraint<IVariable, string> C3 =
        new NotEqualConstraint<IVariable, string>(Q, NSW);

    private static readonly IConstraint<IVariable, string> C4 =
        new NotEqualConstraint<IVariable, string>(NSW, V);

    private IDomain<string> _colors;
    private IList<IVariable> _variables;
    
    [TestInitialize]
    public void TestInitialize()
    {
        _variables = new List<IVariable>
        {
            WA,
            NT,
            Q,
            NSW,
            V
        };

        _colors = new Domain<string>("red", "green", "blue");
    }
    
    [TestMethod]
    public void TestTreeCspSolver()
    {
        var csp = new CSP<IVariable, string>(_variables);
        csp.AddConstraint(C1);
        csp.AddConstraint(C2);
        csp.AddConstraint(C3);
        csp.AddConstraint(C4);

        csp.SetDomain(WA, _colors);
        csp.SetDomain(NT, _colors);
        csp.SetDomain(Q, _colors);
        csp.SetDomain(NSW, _colors);
        csp.SetDomain(V, _colors);

        var solver = new TreeCspSolver<IVariable, string>();
        var assignment = solver.Solve(csp);
        Assert.IsNotNull(assignment);
        Assert.IsTrue(assignment.IsComplete(csp.Variables));
        Assert.IsTrue(assignment.IsSolution(csp));
    }
}