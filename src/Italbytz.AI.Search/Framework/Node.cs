namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Default search node implementation.
/// </summary>
public class Node<TState, TAction> : INode<TState, TAction>
{
    public Node(TState state) : this(state, null, default, 0.0)
    {
    }

    public Node(TState state, INode<TState, TAction>? parent, TAction? action, double pathCost)
    {
        State = state;
        Parent = parent;
        Action = action;
        PathCost = pathCost;
        Depth = parent is not null ? parent.Depth + 1 : 0;
    }

    public TState State { get; }

    public double PathCost { get; }

    public int Depth { get; }

    public TAction? Action { get; }

    public INode<TState, TAction>? Parent { get; }

    public bool IsRootNode() => Parent is null;

    public override string ToString()
    {
        var parentState = Parent is not null ? $"{Parent.State}" : string.Empty;
        return $"[parent={parentState}, action={Action}, state={State}, pathCost={PathCost}]";
    }
}
