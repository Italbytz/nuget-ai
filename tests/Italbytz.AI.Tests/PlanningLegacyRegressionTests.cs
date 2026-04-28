using Italbytz.AI.Planning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Italbytz.AI.Tests;

[TestClass]
public class PlanningLegacyRegressionTests
{
    [TestMethod]
    public void UtilsParseSplitsConjunctionIntoOrderedLiterals()
    {
        var literals = Utils.Parse("At(C1,JFK) ^ At(C2,SFO)");

        Assert.HasCount(2, literals);
        Assert.AreEqual("At(C1,JFK)", literals[0].ToString());
        Assert.AreEqual("At(C2,SFO)", literals[1].ToString());
    }
}