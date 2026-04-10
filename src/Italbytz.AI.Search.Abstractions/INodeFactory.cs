using System;
using System.Collections.Generic;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search;

/// <summary>
/// Creates nodes and expands successor nodes for a search problem.
/// </summary>
public interface INodeFactory<TState, TAction>
{
    INode<TState, TAction> CreateNode(TState state);

    List<INode<TState, TAction>> GetSuccessors(INode<TState, TAction> node, IProblem<TState, TAction> problem);

    bool UseParentLinks { get; set; }

    void AddNodeListener(Action<INode<TState, TAction>> listener);
}
