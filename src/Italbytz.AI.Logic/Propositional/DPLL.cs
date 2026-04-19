using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>
/// DPLL-SATISFIABLE (AIMA3e Fig. 7.17).
/// Backtracking SAT solver with unit-propagation and pure-literal elimination.
/// </summary>
public class DPLL : ISatSolver
{
    public IDictionary<string, bool>? FindModel(IReadOnlyList<IPropClause> clauses)
    {
        var symbols = clauses
            .SelectMany(c => c.Literals.Select(l => l.Symbol))
            .Distinct()
            .ToList();
        var model = new Dictionary<string, bool>();
        return DpllRecursive(clauses.ToList(), symbols, model);
    }

    private static Dictionary<string, bool>? DpllRecursive(
        List<IPropClause> clauses,
        List<string> symbols,
        Dictionary<string, bool> model)
    {
        // Every clause true → satisfiable
        if (clauses.All(c => IsTrue(c, model))) return new Dictionary<string, bool>(model);
        // Some clause false → fail
        if (clauses.Any(c => IsFalse(c, model))) return null;

        // Unit propagation: if a clause has exactly one unassigned literal
        var unitClause = clauses
            .Where(c => !IsTrue(c, model))
            .Select(c => c.Literals.Where(l => !model.ContainsKey(l.Symbol)).ToList())
            .FirstOrDefault(unassigned => unassigned.Count == 1);
        if (unitClause != null)
        {
            var l = unitClause[0];
            var m = new Dictionary<string, bool>(model) { [l.Symbol] = l.IsPositive };
            return DpllRecursive(clauses, symbols.Where(s => s != l.Symbol).ToList(), m);
        }

        // Pure literal elimination
        var remaining = symbols.Where(s => !model.ContainsKey(s)).ToList();
        foreach (var sym in remaining)
        {
            bool hasPos = clauses.Any(c => !IsTrue(c, model) &&
                c.Literals.Any(l => l.Symbol == sym && l.IsPositive));
            bool hasNeg = clauses.Any(c => !IsTrue(c, model) &&
                c.Literals.Any(l => l.Symbol == sym && !l.IsPositive));
            if (hasPos ^ hasNeg)
            {
                var m = new Dictionary<string, bool>(model) { [sym] = hasPos };
                return DpllRecursive(clauses, remaining.Where(s => s != sym).ToList(), m);
            }
        }

        if (!remaining.Any()) return null;

        var first = remaining[0];
        var rest = remaining.Skip(1).ToList();

        var modelTrue = new Dictionary<string, bool>(model) { [first] = true };
        var result = DpllRecursive(clauses, rest, modelTrue);
        if (result != null) return result;

        var modelFalse = new Dictionary<string, bool>(model) { [first] = false };
        return DpllRecursive(clauses, rest, modelFalse);
    }

    private static bool IsTrue(IPropClause clause, IDictionary<string, bool> model) =>
        clause.Literals.Any(l => model.TryGetValue(l.Symbol, out var v) && v == l.IsPositive);

    private static bool IsFalse(IPropClause clause, IDictionary<string, bool> model) =>
        clause.Literals.All(l => model.TryGetValue(l.Symbol, out var v) && v != l.IsPositive);
}
