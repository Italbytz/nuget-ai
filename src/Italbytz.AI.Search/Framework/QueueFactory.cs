using System;

namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Factory for search-specific priority queues.
/// </summary>
public static class QueueFactory
{
    public static NodePriorityQueue<TState, TAction> CreatePriorityQueue<TState, TAction>(Func<INode<TState, TAction>, double> priorityFunction)
    {
        return new NodePriorityQueue<TState, TAction>(priorityFunction);
    }
}
