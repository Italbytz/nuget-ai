using System.Collections.Generic;

namespace Italbytz.EA.SearchSpace;

/// <inheritdoc />
public interface ISetLiteral<TCategory> : ILiteral<TCategory>,
    global::Italbytz.AI.Evolutionary.SearchSpace.ISetLiteral<TCategory>
{
    /// <summary>
    ///     Gets the list of categories contained in this set literal.
    /// </summary>
    /// <value>A list of categories of type <see cref="TCategory" />.</value>
    public new IList<TCategory> Categories { get; }

    /// <summary>
    ///     Gets the feature index associated with this literal.
    /// </summary>
    /// <value>An integer representing the feature index.</value>
    public new int Feature { get; }

    /// <summary>
    ///     Gets the type of this set literal.
    /// </summary>
    /// <value>A <see cref="SetLiteralType" /> value describing the literal variant.</value>
    public new SetLiteralType LiteralType { get; }

    /// <summary>
    ///     Gets the identifier or index of the set this literal refers to.
    /// </summary>
    /// <value>An integer representing the set identifier.</value>
    public new int Set { get; }
}

/// <summary>
///     An enumeration representing the different types of literals used in the
///     LogicGP algorithm.
/// </summary>
/// <remarks>
///     The LogicGpLiteralType enumeration defines the types of literals that can
///     be used in the LogicGP algorithm.
///     Each type of literal has its own characteristics and is used in different
///     contexts.
///     The available types are Dussault, Rudell, Su, and LessGreater.
/// </remarks>
public enum SetLiteralType
{
    /// <summary>Variant as defined by Dussault.</summary>
    Dussault,

    /// <summary>Variant as defined by Rudell.</summary>
    Rudell,

    /// <summary>Variant as defined by Su.</summary>
    Su,

    /// <summary>Variant representing less/greater comparisons.</summary>
    LessGreater
}