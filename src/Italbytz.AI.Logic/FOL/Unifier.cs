using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// UNIFY algorithm (AIMA3e Fig. 9.1) with occurs-check.
/// Unifies two FOL expressions: terms, atomic sentences, or lists of args.
/// </summary>
public class Unifier : IUnifier
{
    public ISubstitution? Unify(object x, object y) =>
        Unify(x, y, Substitution.Empty);

    public ISubstitution? Unify(object x, object y, ISubstitution? theta)
    {
        if (theta is null) return null;
        if (x.Equals(y)) return theta;

        if (x is IVariable vx) return UnifyVar(vx, y, theta);
        if (y is IVariable vy) return UnifyVar(vy, x, theta);

        // Both compound: unify their arg lists
        if (x is IAtomicSentence ax && y is IAtomicSentence ay)
        {
            if (ax.SymbolicName != ay.SymbolicName) return null;
            return UnifyArgs(ax.Args.Cast<object>().ToList(),
                             ay.Args.Cast<object>().ToList(), theta);
        }

        if (x is IFunctionTerm fx && y is IFunctionTerm fy)
        {
            if (fx.SymbolicName != fy.SymbolicName) return null;
            return UnifyArgs(fx.Args.Cast<object>().ToList(),
                             fy.Args.Cast<object>().ToList(), theta);
        }

        return null;
    }

    private ISubstitution? UnifyVar(IVariable var, object x, ISubstitution theta)
    {
        if (theta.Binds(var)) return Unify(theta.GetBinding(var), x, theta);
        if (x is IVariable xv && theta.Binds(xv)) return Unify(var, theta.GetBinding(xv), theta);
        if (OccursIn(var, x, theta)) return null;  // occurs-check
        return x is ITerm term ? theta.Extend(var, term) : null;
    }

    private bool OccursIn(IVariable var, object x, ISubstitution theta)
    {
        if (x is IVariable xv)
        {
            if (xv.Equals(var)) return true;
            if (theta.Binds(xv)) return OccursIn(var, theta.GetBinding(xv), theta);
            return false;
        }
        if (x is IAtomicSentence atom)
            return atom.Args.Any(arg => OccursIn(var, arg, theta));
        if (x is IFunctionTerm function)
            return function.Args.Any(arg => OccursIn(var, arg, theta));
        return false;
    }

    private ISubstitution? UnifyArgs(List<object> args1, List<object> args2, ISubstitution? theta)
    {
        if (theta is null) return null;
        if (args1.Count != args2.Count) return null;
        if (!args1.Any()) return theta;
        var unified = Unify(args1[0], args2[0], theta);
        return UnifyArgs(args1.Skip(1).ToList(), args2.Skip(1).ToList(), unified);
    }
}
