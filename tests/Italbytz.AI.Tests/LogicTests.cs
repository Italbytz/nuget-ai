using Italbytz.AI.Logic.Propositional;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Tests;

[TestClass]
public class LogicTests
{
    // --------------------------------------------------------------- DPLL
    [TestMethod]
    public void DPLL_SatisfiableFormula_FindsModel()
    {
        // (A ∨ B) ∧ (¬A ∨ C)
        var clauses = new List<IPropClause>
        {
            new PropClause(new[] {
                new PropLiteral("A", true),
                new PropLiteral("B", true)
            }),
            new PropClause(new[] {
                new PropLiteral("A", false),
                new PropLiteral("C", true)
            })
        };

        var solver = new DPLL();
        var model = solver.FindModel(clauses);
        Assert.IsNotNull(model, "Expected a satisfying model");
    }

    [TestMethod]
    public void DPLL_UnsatisfiableFormula_ReturnsNull()
    {
        // (A) ∧ (¬A)
        var clauses = new List<IPropClause>
        {
            new PropClause(new[] { new PropLiteral("A", true) }),
            new PropClause(new[] { new PropLiteral("A", false) })
        };

        var solver = new DPLL();
        var model = solver.FindModel(clauses);
        Assert.IsNull(model, "Expected no satisfying model");
    }

    // ------------------------------------------------------------ WalkSAT
    [TestMethod]
    public void WalkSAT_SatisfiableFormula_FindsModel()
    {
        var clauses = new List<IPropClause>
        {
            new PropClause(new[] {
                new PropLiteral("A", true),
                new PropLiteral("B", true)
            }),
            new PropClause(new[] {
                new PropLiteral("A", false),
                new PropLiteral("C", true)
            })
        };

        var solver = new WalkSAT(maxFlips: 1000, p: 0.5, seed: 42);
        var model = solver.FindModel(clauses);
        Assert.IsNotNull(model, "WalkSAT should find a model");
    }

    // --------------------------------------------------------- PLResolution
    [TestMethod]
    public void PLResolution_EntailsSimpleQuery()
    {
        var kb = new PropKnowledgeBase();
        // Modus ponens: P, P=>Q ⊨ Q
        var pSymbol = new PropSymbol("P");
        var qSymbol = new PropSymbol("Q");
        var pImpliesQ = new ConnectedSentence(Connective.IMPLIES, pSymbol, qSymbol);
        kb.Tell(pSymbol);
        kb.Tell(pImpliesQ);

        var checker = new PLResolution();
        Assert.IsTrue(checker.IsEntailed(kb, "Q"));
    }

    [TestMethod]
    public void PLResolution_DoesNotEntailUnrelatedQuery()
    {
        var kb = new PropKnowledgeBase();
        kb.Tell(new PropSymbol("P"));

        var checker = new PLResolution();
        Assert.IsFalse(checker.IsEntailed(kb, "Q"));
    }
}
