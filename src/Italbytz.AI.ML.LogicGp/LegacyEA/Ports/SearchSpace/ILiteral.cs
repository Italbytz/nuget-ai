using System;

namespace Italbytz.EA.SearchSpace;

/// <summary>
///     Represents a literal in a genetic programming context, capable of making
///     predictions on categorical data.
/// </summary>
/// <typeparam name="TCategory">The type of the categorical data.</typeparam>
/// <remarks>
///     A literal is a fundamental component in genetic programming search spaces,
///     particularly for symbolic regression or classification problems.
/// </remarks>
public interface ILiteral<TCategory> : global::Italbytz.AI.Evolutionary.SearchSpace.ILiteral<TCategory>
{
}