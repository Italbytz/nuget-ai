using System;

namespace Italbytz.AI.CSP;

public interface IVariable : IEquatable<IVariable>
{
    string Name { get; }
}
