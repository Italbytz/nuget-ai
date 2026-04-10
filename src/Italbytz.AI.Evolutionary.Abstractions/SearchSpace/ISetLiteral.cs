using System.Collections.Generic;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public interface ISetLiteral<TCategory> : ILiteral<TCategory>
{
    IList<TCategory> Categories { get; }

    int Feature { get; }

    SetLiteralType LiteralType { get; }

    int Set { get; }
}

public enum SetLiteralType
{
    Dussault,
    Rudell,
    Su,
    LessGreater
}
