using System;
using System.Collections.Generic;

namespace Italbytz.AI.Planning.Fol;

public interface IFolNode
{
    string SymbolicName { get; }
}

public interface ISentence : IFolNode
{
}

public interface IAtomicSentence : ISentence
{
    IList<ITerm> Args { get; }
}

public interface IPredicate : IAtomicSentence
{
}

public interface ITerm : IFolNode, IEquatable<ITerm>
{
}

public interface IConstant : ITerm, IEquatable<IConstant>
{
    string Value { get; }
}

public interface IVariable : ITerm, IEquatable<IVariable>
{
    int Indexical { get; }
}
