using System;

namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Minimal priority queue wrapper for search nodes.
/// </summary>
public sealed class NodePriorityQueue<TState, TAction>
{
    private readonly Func<INode<TState, TAction>, double> _priorityFunction;
    private readonly PriorityQueue<INode<TState, TAction>, double> _queue = new();

    public NodePriorityQueue(Func<INode<TState, TAction>, double> priorityFunction)
    {
        _priorityFunction = priorityFunction;
    }

    public int Count => _queue.Count;

    public void Clear() => _queue.Clear();

    public void Enqueue(INode<TState, TAction> node)
    {
        _queue.Enqueue(node, _priorityFunction(node));
    }

    public INode<TState, TAction> Dequeue() => _queue.Dequeue();

    public INode<TState, TAction> Peek() => _queue.Peek();
}
