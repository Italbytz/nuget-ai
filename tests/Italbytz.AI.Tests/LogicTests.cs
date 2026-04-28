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
        var pImpliesQ = new Italbytz.AI.Logic.Propositional.ConnectedSentence(Connective.IMPLIES, pSymbol, qSymbol);
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

    // -------------------------------------------------------------- FOL
    [TestMethod]
    public void Unifier_BindsVariableToConstant()
    {
        var unifier = new Unifier();
        var variable = new Variable("x");
        var constant = new Constant("John");

        var result = unifier.Unify(variable, constant);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Binds(variable));
        Assert.AreEqual(constant, result.GetBinding(variable));
    }

    [TestMethod]
    public void Unifier_UnifiesPredicateArguments()
    {
        var unifier = new Unifier();
        var variable = new Variable("x");
        var left = new Predicate("Knows", new ITerm[] { new Constant("John"), variable });
        var right = new Predicate("Knows", new ITerm[] { new Constant("John"), new Constant("Jane") });

        var result = unifier.Unify(left, right);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Binds(variable));
        Assert.AreEqual(new Constant("Jane"), result.GetBinding(variable));
    }

    [TestMethod]
    public void Unifier_RejectsPredicatesWithDifferentNames()
    {
        var unifier = new Unifier();
        var left = new Predicate("Knows", new ITerm[] { new Constant("John") });
        var right = new Predicate("Likes", new ITerm[] { new Constant("John") });

        var result = unifier.Unify(left, right);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Unifier_RejectsBindingVariableToAtomicSentence()
    {
        var unifier = new Unifier();
        var variable = new Variable("x");
        var sentence = new Predicate("Knows", new ITerm[] { new Constant("John") });

        var result = unifier.Unify(variable, sentence, Substitution.Empty);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Unifier_UnifiesFunctionTerms()
    {
        var unifier = new Unifier();
        var variable = new Variable("x");
        var left = new Function("BrotherOf", new ITerm[] { variable });
        var right = new Function("BrotherOf", new ITerm[] { new Constant("John") });

        var result = unifier.Unify(left, right);

        Assert.IsNotNull(result);
        Assert.AreEqual(new Constant("John"), result.GetBinding(variable));
    }

    [TestMethod]
    public void FolLexer_TokenizesQuantifiersFunctionsAndEquality()
    {
        var tokens = new FolLexer().Tokenize("FORALL x Knows(BrotherOf(x), John) => x = John");

        Assert.AreEqual(FolTokenType.ForAll, tokens[0].Type);
        Assert.AreEqual(FolTokenType.Identifier, tokens[1].Type);
        Assert.AreEqual("Knows", tokens[2].Lexeme);
        Assert.IsTrue(tokens.Any(token => token.Type == FolTokenType.Implies));
        Assert.IsTrue(tokens.Any(token => token.Type == FolTokenType.Equals));
    }

    [TestMethod]
    public void FolParser_ParsesNestedFunctionPredicate()
    {
        var domain = new FolDomain()
            .AddConstant("John")
            .AddFunction("BrotherOf")
            .AddPredicate("Knows");
        var parser = new FolParser(domain);

        var sentence = parser.Parse("Knows(BrotherOf(x), John)");

        Assert.IsInstanceOfType<Predicate>(sentence);
        var predicate = (Predicate)sentence;
        Assert.AreEqual("Knows", predicate.SymbolicName);
        Assert.IsInstanceOfType<Function>(predicate.Args.First());
        Assert.AreEqual(new Constant("John"), predicate.Args.Last());
    }

    [TestMethod]
    public void FolParser_ParsesQuantifiedImplicationWithNegation()
    {
        var domain = new FolDomain()
            .AddConstant("John")
            .AddPredicate("Human", "Mortal");
        var parser = new FolParser(domain);

        var sentence = parser.Parse("FORALL x (Human(x) => ~Mortal(John))");

        Assert.IsInstanceOfType<QuantifiedSentence>(sentence);
        var quantified = (QuantifiedSentence)sentence;
        Assert.AreEqual("FORALL", quantified.Quantifier);
        Assert.AreEqual(new Variable("x"), quantified.Variables.Single());
        Assert.IsInstanceOfType<Italbytz.AI.Planning.Fol.ConnectedSentence>(quantified.Sentence);
        var implication = (Italbytz.AI.Planning.Fol.ConnectedSentence)quantified.Sentence;
        Assert.AreEqual("=>", implication.Connector);
        Assert.IsInstanceOfType<NotSentence>(implication.Second);
    }

    [TestMethod]
    public void FolParser_ParsesTermEquality()
    {
        var domain = new FolDomain().AddConstant("John");
        var parser = new FolParser(domain);

        var sentence = parser.Parse("x = John");

        Assert.IsInstanceOfType<TermEquality>(sentence);
        var equality = (TermEquality)sentence;
        Assert.AreEqual(new Variable("x"), equality.Left);
        Assert.AreEqual(new Constant("John"), equality.Right);
    }

    [TestMethod]
    public void ForwardChaining_DerivesBoundFactFromHornClause()
    {
        var kb = new FolKnowledgeBase(new ForwardChaining());
        var john = new Constant("John");
        var x = new Variable("x");
        kb.Tell(new Literal(new Predicate("King", new ITerm[] { john }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Greedy", new ITerm[] { john }), false), Array.Empty<ILiteral>());
        kb.Tell(
            new Literal(new Predicate("Evil", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("King", new ITerm[] { x }), false),
                new Literal(new Predicate("Greedy", new ITerm[] { x }), false)
            });

    var answer = kb.Ask(new Predicate("Evil", new ITerm[] { x })).Single();

    Assert.AreEqual(john, answer.GetBinding(x));
    }

    [TestMethod]
    public void BackwardChaining_DerivesBoundFactFromHornClause()
    {
        var kb = new FolKnowledgeBase(new BackwardChaining());
        var john = new Constant("John");
        var x = new Variable("x");
        kb.Tell(new Literal(new Predicate("King", new ITerm[] { john }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Greedy", new ITerm[] { john }), false), Array.Empty<ILiteral>());
        kb.Tell(
            new Literal(new Predicate("Evil", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("King", new ITerm[] { x }), false),
                new Literal(new Predicate("Greedy", new ITerm[] { x }), false)
            });

        var answer = kb.Ask(new Predicate("Evil", new ITerm[] { x })).Single();

        Assert.AreEqual(john, answer.GetBinding(x));
    }

    [TestMethod]
    public void ForwardChaining_KingsKnowledgeBase_DerivesJohnAsEvil()
    {
        var kb = BuildKingsKnowledgeBase(new ForwardChaining());

        var answers = kb.Ask(new Predicate("Evil", new ITerm[] { new Variable("x") })).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("John"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void BackwardChaining_KingsKnowledgeBase_ReturnsBothKings()
    {
        var kb = BuildKingsKnowledgeBase(new BackwardChaining());

        var answers = kb.Ask(new Predicate("King", new ITerm[] { new Variable("x") })).ToList();

        Assert.HasCount(2, answers);
        CollectionAssert.AreEquivalent(
            new object[] { new Constant("John"), new Constant("Richard") },
            answers.Select(answer => (object)answer.GetBinding(new Variable("x"))).ToArray());
    }

    [TestMethod]
    public void ForwardChaining_WeaponsKnowledgeBase_DerivesWestAsCriminal()
    {
        var kb = BuildWeaponsKnowledgeBase(new ForwardChaining());

        var answers = kb.Ask(new Predicate("Criminal", new ITerm[] { new Variable("x") })).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("West"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void BackwardChaining_WeaponsKnowledgeBase_DerivesWestAsCriminal()
    {
        var kb = BuildWeaponsKnowledgeBase(new BackwardChaining());

        var answers = kb.Ask(new Predicate("Criminal", new ITerm[] { new Variable("x") })).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("West"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void ForwardChaining_ParsesAndAnswersTextualKingsKnowledgeBase()
    {
        var parser = new FolParser(new FolDomain()
            .AddConstant("John", "Richard")
            .AddPredicate("King", "Greedy", "Evil"));
        var kb = new FolKnowledgeBase(new ForwardChaining());

        kb.Tell("King(John)", parser);
        kb.Tell("King(Richard)", parser);
        kb.Tell("Greedy(John)", parser);
        kb.Tell("King(x) & Greedy(x) => Evil(x)", parser);

        var answers = kb.Ask("Evil(x)", parser).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("John"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void BackwardChaining_ParsesAndAnswersTextualWeaponsKnowledgeBase()
    {
        var parser = new FolParser(new FolDomain()
            .AddConstant("West", "Nono", "America", "M1")
            .AddPredicate("American", "Missile", "Owns", "Enemy", "Weapon", "Hostile", "Sells", "Criminal"));
        var kb = new FolKnowledgeBase(new BackwardChaining());

        kb.Tell("American(West)", parser);
        kb.Tell("Missile(M1)", parser);
        kb.Tell("Owns(Nono, M1)", parser);
        kb.Tell("Enemy(Nono, America)", parser);
        kb.Tell("Missile(x) => Weapon(x)", parser);
        kb.Tell("Enemy(x, America) => Hostile(x)", parser);
        kb.Tell("Missile(x) & Owns(Nono, x) => Sells(West, x, Nono)", parser);
        kb.Tell("American(x) & Weapon(y) & Sells(x, y, z) & Hostile(z) => Criminal(x)", parser);

        var answers = kb.Ask("Criminal(x)", parser).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("West"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void ForwardChaining_ParsesUniversallyQuantifiedTextualClause()
    {
        var parser = new FolParser(new FolDomain()
            .AddConstant("John")
            .AddPredicate("King", "Greedy", "Evil"));
        var kb = new FolKnowledgeBase(new ForwardChaining());

        kb.Tell("King(John)", parser);
        kb.Tell("Greedy(John)", parser);
        kb.Tell("FORALL x (King(x) & Greedy(x) => Evil(x))", parser);

        var answers = kb.Ask("Evil(x)", parser).ToList();

        Assert.HasCount(1, answers);
        Assert.AreEqual(new Constant("John"), answers[0].GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void ForwardChaining_AskResultReportsTruthAndBindingsForAtomicQuery()
    {
        var kb = BuildKingsKnowledgeBase(new ForwardChaining());

        var result = kb.AskResult(new Predicate("Evil", new ITerm[] { new Variable("x") }));

        Assert.IsTrue(result.IsTrue);
        Assert.IsFalse(result.IsPossiblyFalse);
        Assert.IsFalse(result.IsUnknownDueToTimeout);
        Assert.IsFalse(result.IsPartialResultDueToTimeout);
        Assert.HasCount(1, result.Proofs);
        Assert.AreEqual(new Constant("John"), result.Proofs[0].AnswerBindings.GetBinding(new Variable("x")));
        Assert.HasCount(1, result.Proofs[0].Steps);
        Assert.AreEqual("FORWARD_CHAINING", result.Proofs[0].Steps[0].InferenceRule);
        Assert.AreEqual("Evil(x)", result.Proofs[0].Steps[0].Conclusion);
        Assert.HasCount(0, result.Proofs[0].Steps[0].Premises);
    }

    [TestMethod]
    public void BackwardChaining_AskResultReportsPossiblyFalseWhenNoProofExists()
    {
        var kb = BuildKingsKnowledgeBase(new BackwardChaining());

        var result = kb.AskResult(new Predicate("Evil", new ITerm[] { new Constant("Richard") }));

        Assert.IsFalse(result.IsTrue);
        Assert.IsTrue(result.IsPossiblyFalse);
        Assert.IsFalse(result.IsUnknownDueToTimeout);
        Assert.IsFalse(result.IsPartialResultDueToTimeout);
        Assert.HasCount(0, result.Proofs);
    }

    [TestMethod]
    public void ForwardChaining_SupportsTermEqualityQueries()
    {
        var kb = BuildKingsKnowledgeBase(new ForwardChaining());

        var result = kb.AskResult(new TermEquality(new Variable("x"), new Constant("John")));

        Assert.IsTrue(result.IsTrue);
        Assert.HasCount(1, result.Proofs);
        Assert.AreEqual(new Constant("John"), result.Proofs[0].AnswerBindings.GetBinding(new Variable("x")));
    }

    [TestMethod]
    public void ForwardChaining_TermEqualityCanBeDerivedViaEqualityAxiom()
    {
        var kb = BuildEqualityKnowledgeBase(new ForwardChaining());

        var result = kb.AskResult(new TermEquality(new Constant("A"), new Constant("C")));

        Assert.IsTrue(result.IsTrue);
        Assert.IsFalse(result.IsPossiblyFalse);
        Assert.HasCount(1, result.Proofs[0].Steps);
        Assert.AreEqual("FORWARD_CHAINING_EQUALITY", result.Proofs[0].Steps[0].InferenceRule);
    }

    [TestMethod]
    public void BackwardChaining_TermEqualityCanBeDerivedViaEqualityAxiom()
    {
        var kb = BuildEqualityKnowledgeBase(new BackwardChaining());

        var result = kb.AskResult(new TermEquality(new Constant("A"), new Constant("C")));

        Assert.IsTrue(result.IsTrue);
        Assert.IsFalse(result.IsPossiblyFalse);
        Assert.HasCount(1, result.Proofs[0].Steps);
        Assert.AreEqual("BACKWARD_CHAINING_EQUALITY_FALLBACK", result.Proofs[0].Steps[0].InferenceRule);
    }

    [TestMethod]
    public void ForwardChaining_ParsesAndAnswersTextualEqualityRule()
    {
        var parser = new FolParser(new FolDomain().AddConstant("A", "B", "C"));
        var kb = new FolKnowledgeBase(new ForwardChaining());

        kb.Tell("A = B", parser);
        kb.Tell("B = C", parser);
        kb.Tell("A = B & B = C => A = C", parser);

        var answers = kb.Ask("A = C", parser).ToList();

        Assert.HasCount(1, answers);
    }

    [TestMethod]
    public void BackwardChaining_SupportsNegationAsFailureForAtomicQueries()
    {
        var kb = BuildKingsKnowledgeBase(new BackwardChaining());

        var result = kb.AskResult(new NotSentence(new Predicate("Evil", new ITerm[] { new Constant("Richard") })));

        Assert.IsTrue(result.IsTrue);
        Assert.IsFalse(result.IsPossiblyFalse);
        Assert.HasCount(1, result.Proofs);
        Assert.IsTrue(result.Proofs[0].AnswerBindings.IsEmpty);
        Assert.HasCount(1, result.Proofs[0].Steps);
        Assert.AreEqual("NEGATION_AS_FAILURE", result.Proofs[0].Steps[0].InferenceRule);
        Assert.AreEqual("~Evil(Richard)", result.Proofs[0].Steps[0].Conclusion);
        CollectionAssert.AreEqual(new[] { "Evil(Richard)" }, result.Proofs[0].Steps[0].Premises.ToArray());
    }

    [TestMethod]
    public void BackwardChaining_NegatedTrueAtomicQueryIsPossiblyFalse()
    {
        var kb = BuildKingsKnowledgeBase(new BackwardChaining());

        var result = kb.AskResult(new NotSentence(new Predicate("Evil", new ITerm[] { new Constant("John") })));

        Assert.IsFalse(result.IsTrue);
        Assert.IsTrue(result.IsPossiblyFalse);
    }

    [TestMethod]
    public void Tell_TextualClauseWithExistentialQuantifier_IsRejected()
    {
        var parser = new FolParser(new FolDomain().AddPredicate("King"));
        var kb = new FolKnowledgeBase(new ForwardChaining());

        ArgumentException? exception = null;
        try
        {
            kb.Tell("EXISTS x King(x)", parser);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "universally quantified");
    }

    private static FolKnowledgeBase BuildKingsKnowledgeBase(IFolInference inference)
    {
        var kb = new FolKnowledgeBase(inference);
        var x = new Variable("x");

        kb.Tell(new Literal(new Predicate("King", new ITerm[] { new Constant("John") }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("King", new ITerm[] { new Constant("Richard") }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Greedy", new ITerm[] { new Constant("John") }), false), Array.Empty<ILiteral>());
        kb.Tell(
            new Literal(new Predicate("Evil", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("King", new ITerm[] { x }), false),
                new Literal(new Predicate("Greedy", new ITerm[] { x }), false)
            });

        return kb;
    }

    private static FolKnowledgeBase BuildWeaponsKnowledgeBase(IFolInference inference)
    {
        var kb = new FolKnowledgeBase(inference);
        var x = new Variable("x");
        var y = new Variable("y");
        var z = new Variable("z");
        var america = new Constant("America");
        var west = new Constant("West");
        var nono = new Constant("Nono");
        var m1 = new Constant("M1");

        kb.Tell(new Literal(new Predicate("American", new ITerm[] { west }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Missile", new ITerm[] { m1 }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Owns", new ITerm[] { nono, m1 }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("Enemy", new ITerm[] { nono, america }), false), Array.Empty<ILiteral>());

        kb.Tell(
            new Literal(new Predicate("Weapon", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("Missile", new ITerm[] { x }), false)
            });
        kb.Tell(
            new Literal(new Predicate("Hostile", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("Enemy", new ITerm[] { x, america }), false)
            });
        kb.Tell(
            new Literal(new Predicate("Sells", new ITerm[] { west, x, nono }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("Missile", new ITerm[] { x }), false),
                new Literal(new Predicate("Owns", new ITerm[] { nono, x }), false)
            });
        kb.Tell(
            new Literal(new Predicate("Criminal", new ITerm[] { x }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("American", new ITerm[] { x }), false),
                new Literal(new Predicate("Weapon", new ITerm[] { y }), false),
                new Literal(new Predicate("Sells", new ITerm[] { x, y, z }), false),
                new Literal(new Predicate("Hostile", new ITerm[] { z }), false)
            });

        return kb;
    }

    private static FolKnowledgeBase BuildEqualityKnowledgeBase(IFolInference inference)
    {
        var kb = new FolKnowledgeBase(inference);
        var x = new Variable("x");
        var y = new Variable("y");
        var z = new Variable("z");
        var a = new Constant("A");
        var b = new Constant("B");
        var c = new Constant("C");

        kb.Tell(new Literal(new Predicate("=", new ITerm[] { a, b }), false), Array.Empty<ILiteral>());
        kb.Tell(new Literal(new Predicate("=", new ITerm[] { b, c }), false), Array.Empty<ILiteral>());
        kb.Tell(
            new Literal(new Predicate("=", new ITerm[] { x, z }), false),
            new ILiteral[]
            {
                new Literal(new Predicate("=", new ITerm[] { x, y }), false),
                new Literal(new Predicate("=", new ITerm[] { y, z }), false)
            });

        return kb;
    }
}
