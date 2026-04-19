using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.MDP;

/// <summary>
/// VALUE ITERATION (AIMA3e Fig. 17.4).
/// Computes the optimal utility of every state and extracts the greedy policy.
/// Converges when max |U(s) - U'(s)| &lt; ε * (1 - γ) / γ.
/// </summary>
public class ValueIteration<TState, TAction> : IMdpSolver<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly double _epsilon;
    private readonly int _maxIterations;

    public IMetrics Metrics { get; } = new Metrics();

    public ValueIteration(double epsilon = 0.001, int maxIterations = 10_000)
    {
        _epsilon = epsilon;
        _maxIterations = maxIterations;
    }

    public (IPolicy<TState, TAction> Policy, IReadOnlyDictionary<TState, double> Utilities)
        Solve(IMarkovDecisionProcess<TState, TAction> mdp)
    {
        double threshold = _epsilon * (1.0 - mdp.Discount) / mdp.Discount;
        var U = mdp.States.ToDictionary(s => s, _ => 0.0);
        int iterations = 0;

        while (true)
        {
            var UPrime = new Dictionary<TState, double>(U);
            double delta = 0;

            foreach (var s in mdp.States)
            {
                var actions = mdp.Actions(s).ToList();
                if (!actions.Any()) { UPrime[s] = mdp.Reward(s); continue; }

                double best = actions.Max(a =>
                    mdp.States.Sum(sPrime =>
                        mdp.Transition(s, a, sPrime) * U[sPrime]));

                UPrime[s] = mdp.Reward(s) + mdp.Discount * best;
                delta = Math.Max(delta, Math.Abs(UPrime[s] - U[s]));
            }

            U = UPrime;
            iterations++;

            if (delta < threshold || iterations >= _maxIterations)
                break;
        }

        Metrics.Set("iterations", iterations);

        var policy = new LambdaPolicy<TState, TAction>(s =>
        {
            var actions = mdp.Actions(s).ToList();
            if (!actions.Any()) return default;
            return actions.MaxBy(a =>
                mdp.States.Sum(sPrime => mdp.Transition(s, a, sPrime) * U[sPrime]));
        });

        return (Policy: policy, Utilities: U);
    }
}
