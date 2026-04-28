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

public interface IFunctionTerm : ITerm
{
    IList<ITerm> Args { get; }
}

public interface IVariable : ITerm, IEquatable<IVariable>
{
    int Indexical { get; }
}

public interface INotSentence : ISentence
{
    ISentence Negated { get; }
}

public interface IConnectedSentence : ISentence
{
    string Connector { get; }

    ISentence First { get; }

    ISentence Second { get; }
}

public interface IQuantifiedSentence : ISentence
{
    string Quantifier { get; }

    IList<IVariable> Variables { get; }

    ISentence Sentence { get; }
}

public interface ITermEquality : ISentence
{
    ITerm Left { get; }

    ITerm Right { get; }
}
