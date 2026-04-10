namespace Italbytz.AI.Search;

/// <summary>
/// Represents a node in a search tree.
/// </summary>
/// <typeparam name="TState">Type used to represent states.</typeparam>
/// <typeparam name="TAction">Type used to represent actions.</typeparam>
public interface INode<TState, TAction>
{
    TState State { get; }

    double PathCost { get; }

    int Depth { get; }

    TAction? Action { get; }

    INode<TState, TAction>? Parent { get; }

    bool IsRootNode();
}
