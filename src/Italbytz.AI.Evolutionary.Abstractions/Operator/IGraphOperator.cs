using System.Collections.Generic;

namespace Italbytz.AI.Evolutionary.Operator;

public interface IGraphOperator : IOperator
{
    int MaxParents { get; }

    int MaxChildren { get; }

    IReadOnlyList<IGraphOperator> Children { get; }

    IReadOnlyList<IGraphOperator> Parents { get; }

    void Check();

    void AddChildren(params IGraphOperator[] children);
}