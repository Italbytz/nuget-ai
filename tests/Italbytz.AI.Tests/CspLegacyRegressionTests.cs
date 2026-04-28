using Italbytz.AI.CSP;
using Italbytz.AI.CSP.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Italbytz.AI.Tests;

[TestClass]
public class CspLegacyRegressionTests
{
    [TestMethod]
    public void AssignmentTracksCompletionAcrossAddAndRemove()
    {
        IVariable x = new Variable("x");
        IVariable y = new Variable("y");
        var variables = new[] { x, y };
        var assignment = new Assignment<IVariable, string>();

        assignment.Add(x, "red");

        Assert.AreEqual("red", assignment.GetValue(x));
        Assert.IsFalse(assignment.IsComplete(variables));

        assignment.Add(y, "blue");

        Assert.IsTrue(assignment.IsComplete(variables));

        assignment.Remove(y);

        Assert.IsFalse(assignment.IsComplete(variables));
        Assert.IsNull(assignment.GetValue(y));
    }

    [TestMethod]
    public void CspRemovesConstraintsAndKeepsCopiedDomainsIndependent()
    {
        IVariable x = new Variable("x");
        IVariable y = new Variable("y");
        IVariable z = new Variable("z");
        var csp = new CSP<IVariable, string>([x, y, z]);
        var firstConstraint = new NotEqualConstraint<IVariable, string>(x, y);
        var secondConstraint = new NotEqualConstraint<IVariable, string>(x, y);
        var colors = new Domain<string>("red", "green", "blue");
        var animals = new Domain<string>("cat", "dog");

        csp.AddConstraint(firstConstraint);
        csp.AddConstraint(secondConstraint);
        csp.SetDomain(x, colors);

        Assert.HasCount(2, csp.Constraints);
        Assert.HasCount(2, csp.GetConstraints(x));
        Assert.HasCount(2, csp.GetConstraints(y));
        Assert.IsEmpty(csp.GetConstraints(z));
        Assert.AreEqual(y, csp.GetNeighbor(x, firstConstraint));
        Assert.AreEqual(x, csp.GetNeighbor(y, firstConstraint));

        Assert.IsTrue(csp.RemoveConstraint(firstConstraint));
        Assert.IsFalse(csp.RemoveConstraint(firstConstraint));
        Assert.HasCount(1, csp.Constraints);

        var copy = csp.CopyDomains();
        Assert.IsTrue(copy.RemoveValueFromDomain(x, "red"));
        copy.SetDomain(x, animals);

        CollectionAssert.AreEqual(new[] { "cat", "dog" }, copy.GetDomain(x).ToList());
        CollectionAssert.AreEqual(new[] { "red", "green", "blue" }, csp.GetDomain(x).ToList());
    }
}