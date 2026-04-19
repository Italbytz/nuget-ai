using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.MDP;

/// <summary>
/// POLICY ITERATION (AIMA3e Fig. 17.7).
/// Alternates between policy evaluation (via iteration) and greedy policy improvement.
/// Often converges in fewer iterations than value iteration.
/// </summary>
public class PolicyIteration<TState, TAction> : IMdpSolver<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly int _evalRounds;
    private readonly int _maxIterations;

    public IMetrics Metrics { get; } = new Metrics();

    public PolicyIteration(int evalRounds = 20, int maxIterations = 1_000)
    {
        _evalRounds = evalRounds;
        _maxIterations = maxIterations;
    }

    public (IPolicy<TState, TAction> Policy, IReadOnlyDictionary<TState, double> Utilities)
        Solve(IMarkovDecisionProcess<TState, TAction> mdp)
    {
        var states = mdp.States.ToList();
        var U = states.ToDictionary(s => s, _ => 0.0);

        // Initialise with first available action per state
        var pi = states.ToDictionary(
            s => s,
            s => mdp.Actions(s).FirstOrDefault()!);

        int iterations = 0;

        while (true)
        {
            U = PolicyEvaluate(pi, U, mdp);
            bool unchanged = true;

            foreach (var s in states)
            {
                var actions = mdp.Actions(s).ToList();
                if (!actions.Any()) continue;

                double qPi = Q(s, pi[s], U, mdp);
                var bestAction = actions.MaxBy(a => Q(s, a, U, mdp))!;
                double qBest = Q(s, bestAction, U, mdp);

                if (qBest > qPi)
                {
                    pi[s] = bestAction;
                    unchanged = false;
                }
            }

            iterations++;
            if (unchanged || iterations >= _maxIterations) break;
        }

        Metrics.Set("iterations", iterations);

        var frozen = new Dictionary<TState, TAction>(pi);
        return (Policy:
            new LambdaPolicy<TState, TAction>(s =>
                frozen.TryGetValue(s, out var a) ? a : default),
            Utilities: U
        );
    }

    private Dictionary<TState, double> PolicyEvaluate(
        Dictionary<TState, TAction> pi,
        Dictionary<TState, double> U,
        IMarkovDecisionProcess<TState, TAction> mdp)
    {
        var result = new Dictionary<TState, double>(U);
        for (int k = 0; k < _evalRounds; k++)
        {
            var next = new Dictionary<TState, double>(result);
            foreach (var s in mdp.States)
            {
                if (pi[s] is null) { next[s] = mdp.Reward(s); continue; }
                double val = mdp.States.Sum(sPrime =>
                    mdp.Transition(s, pi[s], sPrime) * result[sPrime]);
                next[s] = mdp.Reward(s) + mdp.Discount * val;
            }
            result = next;
        }
        return result;
    }

    private static double Q(
        TState s,
        TAction a,
        Dictionary<TState, double> U,
        IMarkovDecisionProcess<TState, TAction> mdp) =>
        mdp.States.Sum(sPrime => mdp.Transition(s, a, sPrime) * U[sPrime]);
}
