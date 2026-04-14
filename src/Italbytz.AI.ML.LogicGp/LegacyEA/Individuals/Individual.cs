using System;
using System.IO;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.AI;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividual" />
public class Individual : global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
    IIndividual, ISaveable
{
    public Individual(global::Italbytz.AI.Evolutionary.Individuals.IGenotype genotype,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual[]? parents)
    {
        Genotype = genotype;
        Parents = parents;
    }

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividual[]? Parents { get; set; }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Individuals.IGenotype Genotype { get; }

    IGenotype IIndividual.Genotype =>
        Genotype as IGenotype
        ?? throw new InvalidOperationException(
            "Genotype must use a LegacyEA implementation.");

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? LatestKnownFitness
    {
        get => Genotype.LatestKnownFitness;
        set => Genotype.LatestKnownFitness = value;
    }

    /// <inheritdoc />
    public int Size => Genotype.Size;

    /// <inheritdoc />
    public int Generation { get; set; }

    /// <inheritdoc />
    public bool IsDominating(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual otherIndividual)
    {
        var fitness = LatestKnownFitness;
        var otherFitness = otherIndividual.LatestKnownFitness;
        if (fitness == null || otherFitness == null)
            throw new InvalidOperationException("Fitness not set");
        return fitness.IsDominating(otherFitness);
    }

    /// <inheritdoc />
    public object Clone()
    {
        return new Individual(
            (global::Italbytz.AI.Evolutionary.Individuals.IGenotype)Genotype.Clone(),
            [this])
        {
            Generation = Generation
        };
    }

    public void Save(Stream stream)
    {
        if (Genotype is ISaveable saveable)
            saveable.Save(stream);
        else
            throw new InvalidOperationException("Genotype is not saveable.");
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Genotype +
               $" Generation {Generation}, " +
               $"Fitness {LatestKnownFitness}" ??
               string.Empty;
    }
}