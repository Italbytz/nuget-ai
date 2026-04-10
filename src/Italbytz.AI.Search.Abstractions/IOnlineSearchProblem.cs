using System;
using System.Collections.Generic;

namespace Italbytz.AI.Problem;

/// <summary>
/// Describes an online search problem.
/// </summary>
public interface IOnlineSearchProblem<TState, TAction>
{
    TState InitialState { get; }

    Func<TState, List<TAction>> Actions { get; }

    Func<TState, bool> GoalTest { get; }

    Func<TState, TAction, TState, double> StepCosts { get; }
}
