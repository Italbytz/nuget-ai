using System;

namespace Italbytz.EA.Operator;

internal class Start : GraphOperator
{
    public override int MaxParents { get; } = 0;
    public override int MaxChildren { get; } = int.MaxValue;

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}