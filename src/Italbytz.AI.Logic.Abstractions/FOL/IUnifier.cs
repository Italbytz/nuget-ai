namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// Computes the most-general unifier of two expressions (AIMA3e Fig. 9.1).
/// Returns null if unification fails.
/// </summary>
public interface IUnifier
{
    ISubstitution? Unify(object x, object y);
    ISubstitution? Unify(object x, object y, ISubstitution? theta);
}
