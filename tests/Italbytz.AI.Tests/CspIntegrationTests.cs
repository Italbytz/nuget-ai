using Italbytz.AI.CSP;
using Italbytz.AI.CSP.Examples;
using Italbytz.AI.CSP.Solver;

namespace Italbytz.AI.Tests;

[TestClass]
public class CspIntegrationTests
{
    [TestMethod]
    public void Csp_tracks_constraints_and_domain_copies_independently()
    {
        IVariable x = new Variable("x");
        IVariable y = new Variable("y");
        IVariable z = new Variable("z");

        var csp = new CSP<IVariable, string>([x, y, z]);
        var colors = new Domain<string>("red", "green", "blue");
        var constraint = new NotEqualConstraint<IVariable, string>(x, y);

        csp.AddConstraint(constraint);
        csp.SetDomain(x, colors);

        Assert.HasCount(1, csp.Constraints);
        Assert.HasCount(1, csp.GetConstraints(x));
        Assert.IsEmpty(csp.GetConstraints(z));
        Assert.AreEqual(y, csp.GetNeighbor(x, constraint));
        Assert.AreEqual(3, csp.GetDomain(x).Count);

        var cspCopy = csp.CopyDomains();
        cspCopy.RemoveValueFromDomain(x, "red");

        Assert.AreEqual(2, cspCopy.GetDomain(x).Count);
        Assert.AreEqual(3, csp.GetDomain(x).Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Backtracking_and_min_conflicts_solve_map_coloring_problem()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        try
        {
            var csp = new MapCSP();
            var backtracking = new FlexibleBacktrackingSolver<Variable, string>();
            var minConflicts = new MinConflictsSolver<Variable, string>(200);

            var backtrackingAssignment = backtracking.Solve(csp);
            var minConflictAssignment = minConflicts.Solve(new MapCSP());

            Assert.IsNotNull(backtrackingAssignment);
            Assert.IsTrue(backtrackingAssignment.IsSolution(csp));
            Assert.IsNotNull(minConflictAssignment);
            Assert.IsTrue(minConflictAssignment.IsSolution(new MapCSP()));
        }
        finally
        {
            ThreadSafeRandomNetCore.Seed = null;
        }
    }

    [TestMethod]
    public void Heuristic_backtracking_solves_legacy_map_coloring_problem()
    {
        var csp = new MapCSP();
        var solver = new FlexibleBacktrackingSolver<Variable, string>
        {
            VariableSelectionStrategy = CspHeuristics.MrvDeg<Variable, string>(),
            ValueOrderingStrategy = CspHeuristics.Lcv<Variable, string>()
        };

        var assignment = solver.Solve(csp);

        Assert.IsNotNull(assignment);
        Assert.IsTrue(assignment.IsComplete(csp.Variables));
        Assert.IsTrue(assignment.IsSolution(csp));

        foreach (var variable in csp.Variables)
        {
            CollectionAssert.Contains(MapCSP.Colors.ToList(), assignment.GetValue(variable));
        }
    }

    [TestMethod]
    public void Tree_solver_handles_tree_structured_csp()
    {
        IVariable wa = new Variable("WA");
        IVariable nt = new Variable("NT");
        IVariable q = new Variable("Q");
        IVariable nsw = new Variable("NSW");
        IVariable v = new Variable("V");

        var colors = new Domain<string>("red", "green", "blue");
        var csp = new CSP<IVariable, string>([wa, nt, q, nsw, v]);
        csp.AddConstraint(new NotEqualConstraint<IVariable, string>(wa, nt));
        csp.AddConstraint(new NotEqualConstraint<IVariable, string>(nt, q));
        csp.AddConstraint(new NotEqualConstraint<IVariable, string>(q, nsw));
        csp.AddConstraint(new NotEqualConstraint<IVariable, string>(nsw, v));
        csp.SetDomain(wa, colors);
        csp.SetDomain(nt, colors);
        csp.SetDomain(q, colors);
        csp.SetDomain(nsw, colors);
        csp.SetDomain(v, colors);

        var assignment = new TreeCspSolver<IVariable, string>().Solve(csp);

        Assert.IsNotNull(assignment);
        Assert.IsTrue(assignment.IsComplete(csp.Variables));
        Assert.IsTrue(assignment.IsSolution(csp));
    }
}
