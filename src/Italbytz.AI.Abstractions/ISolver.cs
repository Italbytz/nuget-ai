namespace Italbytz.AI.Abstractions;

/// <summary>
/// Represents a generic solver that produces a solution for a given set of parameters.
/// </summary>
/// <typeparam name="TParameters">The input parameters.</typeparam>
/// <typeparam name="TSolution">The produced solution.</typeparam>
public interface ISolver<in TParameters, out TSolution>
{
    TSolution Solve(TParameters parameters);
}
