using System;

namespace Italbytz.EA.PopulationManager;

/// <inheritdoc cref="global::Italbytz.AI.Evolutionary.PopulationManager.IPopulationManager" />
internal class DefaultPopulationManager : global::Italbytz.AI.Evolutionary.PopulationManager.IPopulationManager
{
    public void Freeze()
    {
        if (Population == null)
            throw new InvalidOperationException(
                "Population is not initialized.");
        foreach (var individual in Population)
            if (individual.Genotype is global::Italbytz.AI.Evolutionary.Individuals.IFreezable freezable)
                freezable.Freeze();
    }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList? Population { get; set; }

    /// <inheritdoc />
    public void InitPopulation(global::Italbytz.AI.Evolutionary.Initialization.IInitialization initialization)
    {
        Population = initialization.Process(null!, null!).Result;
    }

    public string GetPopulationInfo()
    {
        /*return Population?.GetRandomIndividual().ToString() ??
               "Population not initialized.";*/
        return Population?.ToString() ??
               "Population not initialized.";
    }
}