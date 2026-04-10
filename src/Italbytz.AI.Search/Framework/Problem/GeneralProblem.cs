using System;
using System.Collections.Generic;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Framework.Problem;

/// <summary>
/// General-purpose problem implementation composed from delegates.
/// </summary>
public class GeneralProblem<TState, TAction> : IProblem<TState, TAction>
{
    public GeneralProblem(
        TState initialState,
        Func<TState, List<TAction>> actions,
        Func<TState, TAction, TState> result,
        Func<TState, bool> goalTest,
        Func<TState, TAction, TState, double>? stepCosts = null)
    {
        InitialState = initialState;
        Actions = actions;
        Result = result;
        GoalTest = goalTest;
        StepCosts = stepCosts ?? ((_, _, _) => 1.0);
    }

    public TState InitialState { get; }

    public Func<TState, List<TAction>> Actions { get; }

    public Func<TState, TAction, TState> Result { get; }

    public Func<TState, bool> GoalTest { get; }

    public Func<TState, TAction, TState, double> StepCosts { get; }

    public bool TestSolution(INode<TState, TAction> node)
    {
        return GoalTest(node.State);
    }
}
