using System;
using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public interface IActionSchema : IEquatable<IActionSchema>
{
    string Name { get; }

    List<ITerm> Variables { get; }

    IList<ILiteral> Precondition { get; }

    IList<ILiteral> Effect { get; }
}
