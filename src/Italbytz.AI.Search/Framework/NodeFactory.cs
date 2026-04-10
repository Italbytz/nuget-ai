using System;
using System.Collections.Generic;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Default node factory that expands successors by applying the problem's action and result functions.
/// </summary>
public class NodeFactory<TState, TAction> : INodeFactory<TState, TAction>
{
    private readonly List<Action<INode<TState, TAction>>> _listeners = [];

    public bool UseParentLinks { get; set; } = true;

    public void AddNodeListener(Action<INode<TState, TAction>> listener)
    {
        _listeners.Add(listener);
    }

    public INode<TState, TAction> CreateNode(TState state)
    {
        return new Node<TState, TAction>(state);
    }

    public List<INode<TState, TAction>> GetSuccessors(INode<TState, TAction> node, IProblem<TState, TAction> problem)
    {
        var successors = new List<INode<TState, TAction>>();

        foreach (var action in problem.Actions(node.State))
        {
            var successorState = problem.Result(node.State, action);
            var stepCost = problem.StepCosts(node.State, action, successorState);
            var parent = UseParentLinks ? node : null;
            successors.Add(new Node<TState, TAction>(successorState, parent, action, node.PathCost + stepCost));
        }

        foreach (var listener in _listeners)
        {
            listener(node);
        }

        return successors;
    }
}
