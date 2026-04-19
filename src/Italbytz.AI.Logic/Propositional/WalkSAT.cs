using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>
/// WALKSAT (AIMA3e Fig. 7.18).
/// Stochastic local search for SAT. With probability p, flips a random symbol
/// in an unsatisfied clause; otherwise flips the symbol that minimises the number
/// of newly violated clauses.
/// </summary>
public class WalkSAT : ISatSolver
{
    private readonly int _maxFlips;
    private readonly double _p;
    private readonly Random _rng;

    public WalkSAT(int maxFlips = 100_000, double p = 0.5, int? seed = null)
    {
        _maxFlips = maxFlips;
        _p = p;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public IDictionary<string, bool>? FindModel(IReadOnlyList<IPropClause> clauses)
    {
        var symbols = clauses
            .SelectMany(c => c.Literals.Select(l => l.Symbol))
            .Distinct()
            .ToList();

        if (!symbols.Any()) return new Dictionary<string, bool>();

        // Random initial assignment
        var model = symbols.ToDictionary(s => s, _ => _rng.Next(2) == 1);

        for (int flip = 0; flip < _maxFlips; flip++)
        {
            if (clauses.All(c => IsTrue(c, model))) return model;

            var unsatisfied = clauses.Where(c => !IsTrue(c, model)).ToList();
            var clause = unsatisfied[_rng.Next(unsatisfied.Count)];

            string symbol;
            if (_rng.NextDouble() < _p)
            {
                // Random flip
                symbol = clause.Literals[_rng.Next(clause.Literals.Count)].Symbol;
            }
            else
            {
                // Greedy flip: choose symbol that satisfies the most clauses
                symbol = clause.Literals
                    .Select(l => l.Symbol)
                    .OrderByDescending(s =>
                    {
                        model[s] = !model[s];
                        int sat = clauses.Count(c => IsTrue(c, model));
                        model[s] = !model[s];
                        return sat;
                    })
                    .First();
            }

            model[symbol] = !model[symbol];
        }

        return null;
    }

    private static bool IsTrue(IPropClause clause, IDictionary<string, bool> model) =>
        clause.Literals.Any(l => model.TryGetValue(l.Symbol, out var v) && v == l.IsPositive);
}
