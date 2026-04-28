using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

internal static class SubstitutionApplier
{
    public static IAtomicSentence Apply(IAtomicSentence atom, ISubstitution substitution)
    {
        return new Predicate(atom.SymbolicName, atom.Args.Select(arg => Apply(arg, substitution)).ToList());
    }

    public static ITerm Apply(ITerm term, ISubstitution substitution)
    {
        if (term is IVariable variable)
        {
            return substitution.Binds(variable)
                ? Apply(substitution.GetBinding(variable), substitution)
                : term;
        }

        if (term is IFunctionTerm function)
        {
            return new Function(function.SymbolicName, function.Args.Select(arg => Apply(arg, substitution)).ToList());
        }

        return term;
    }
}