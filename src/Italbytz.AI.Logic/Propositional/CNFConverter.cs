using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>
/// Converts a propositional sentence to Conjunctive Normal Form.
/// Follows the standard AIMA3e transformations (p. 253):
/// 1. Eliminate BICONDITIONAL: (α↔β) → (α⇒β)∧(β⇒α)
/// 2. Eliminate IMPLIES: (α⇒β) → (¬α∨β)
/// 3. Move NOT inward using De Morgan, double-negation
/// 4. Distribute OR over AND
/// </summary>
public static class CNFConverter
{
    public static IReadOnlyList<IPropClause> Convert(IPropSentence sentence)
    {
        var nnf = ToNNF(EliminateImplications(sentence));
        return DistributeAndCollect(nnf);
    }

    private static IPropSentence EliminateImplications(IPropSentence s)
    {
        if (s is not ConnectedSentence cs) return s;
        var left = EliminateImplications(cs.Left);
        var right = cs.Right is not null ? EliminateImplications(cs.Right) : null;
        return cs.Connective switch
        {
            Connective.BICONDITIONAL =>
                new ConnectedSentence(Connective.AND,
                    new ConnectedSentence(Connective.OR,
                        new ConnectedSentence(Connective.NOT, left), right!),
                    new ConnectedSentence(Connective.OR,
                        new ConnectedSentence(Connective.NOT, right!), left)),
            Connective.IMPLIES =>
                new ConnectedSentence(Connective.OR,
                    new ConnectedSentence(Connective.NOT, left), right!),
            _ => new ConnectedSentence(cs.Connective, left, right)
        };
    }

    private static IPropSentence ToNNF(IPropSentence s)
    {
        if (s is not ConnectedSentence cs) return s;
        if (cs.Connective == Connective.NOT)
        {
            var inner = cs.Left;
            if (inner is ConnectedSentence ics)
            {
                return ics.Connective switch
                {
                    Connective.NOT => ToNNF(ics.Left),
                    Connective.AND => ToNNF(new ConnectedSentence(Connective.OR,
                        new ConnectedSentence(Connective.NOT, ics.Left),
                        new ConnectedSentence(Connective.NOT, ics.Right!))),
                    Connective.OR => ToNNF(new ConnectedSentence(Connective.AND,
                        new ConnectedSentence(Connective.NOT, ics.Left),
                        new ConnectedSentence(Connective.NOT, ics.Right!))),
                    _ => cs
                };
            }
            return cs;
        }
        return new ConnectedSentence(cs.Connective, ToNNF(cs.Left),
            cs.Right is not null ? ToNNF(cs.Right) : null);
    }

    private static IReadOnlyList<IPropClause> DistributeAndCollect(IPropSentence s)
    {
        if (s is ConnectedSentence cs && cs.Connective == Connective.AND)
        {
            var left = DistributeAndCollect(cs.Left);
            var right = DistributeAndCollect(cs.Right!);
            return left.Concat(right).ToList();
        }

        var literals = CollectLiterals(s);
        var clause = new PropClause(literals);
        return clause.IsTautology ? new List<IPropClause>() : new List<IPropClause> { clause };
    }

    private static IEnumerable<IPropLiteral> CollectLiterals(IPropSentence s)
    {
        if (s is PropSymbol sym)
            return new[] { new PropLiteral(sym.Name, true) };
        if (s is PropLiteral lit)
            return new[] { lit };
        if (s is ConnectedSentence { Connective: Connective.NOT, Left: PropSymbol negSym })
            return new[] { new PropLiteral(negSym.Name, false) };
        if (s is ConnectedSentence cs && cs.Connective == Connective.OR)
            return CollectLiterals(cs.Left).Concat(CollectLiterals(cs.Right!));
        return new List<IPropLiteral>();
    }
}
