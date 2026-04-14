using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA;

/// <inheritdoc cref="global::Italbytz.AI.Evolutionary.IGeneticProgram" />
internal class GeneticProgram : global::Italbytz.AI.Evolutionary.IGeneticProgram,
    global::Italbytz.AI.Evolutionary.StoppingCriterion.IGenerationProvider
{
    private EvolutionaryAlgorithm _ea;

    global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction
        global::Italbytz.AI.Evolutionary.IGeneticProgram.FitnessFunction
    {
        get => FitnessFunction;
        set => FitnessFunction = value;
    }

    global::Italbytz.AI.Evolutionary.Operator.IGraphOperator
        global::Italbytz.AI.Evolutionary.IGeneticProgram.SelectionForOperator
    {
        get => SelectionForOperator;
        set => SelectionForOperator = value;
    }

    global::Italbytz.AI.Evolutionary.Operator.IGraphOperator
        global::Italbytz.AI.Evolutionary.IGeneticProgram.SelectionForSurvival
    {
        get => SelectionForSurvival;
        set => SelectionForSurvival = value;
    }

    IReadOnlyList<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator>
        global::Italbytz.AI.Evolutionary.IGeneticProgram.Mutations
    {
        get => Mutations;
        set => Mutations = value.ToList();
    }

    IReadOnlyList<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator>
        global::Italbytz.AI.Evolutionary.IGeneticProgram.Crossovers
    {
        get => Crossovers;
        set => Crossovers = value.ToList();
    }

    global::Italbytz.AI.Evolutionary.Initialization.IInitialization
        global::Italbytz.AI.Evolutionary.IGeneticProgram.Initialization
    {
        get => Initialization;
        set => Initialization = value;
    }

    global::Italbytz.AI.Evolutionary.PopulationManager.IPopulationManager
        global::Italbytz.AI.Evolutionary.IGeneticProgram.PopulationManager
    {
        get => PopulationManager;
        set => PopulationManager = value;
    }

    global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace
        global::Italbytz.AI.Evolutionary.IGeneticProgram.SearchSpace
    {
        get => SearchSpace;
        set => SearchSpace = value;
    }

    IReadOnlyList<global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion>
        global::Italbytz.AI.Evolutionary.IGeneticProgram.StoppingCriteria
    {
        get => StoppingCriteria;
        set => StoppingCriteria = value.ToArray();
    }

    global::Italbytz.AI.Evolutionary.Individuals.IIndividualList
        global::Italbytz.AI.Evolutionary.IGeneticProgram.Population
        => Population;

    /// <inheritdoc />
    public required global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction FitnessFunction { get; set; }

    /// <inheritdoc />
    public required global::Italbytz.AI.Evolutionary.Operator.IGraphOperator SelectionForOperator { get; set; }

    /// <inheritdoc />
    public required global::Italbytz.AI.Evolutionary.Operator.IGraphOperator SelectionForSurvival { get; set; }

    /// <inheritdoc />
    public required List<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator> Mutations { get; set; }

    /// <inheritdoc />
    public required List<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator> Crossovers { get; set; }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Initialization.IInitialization Initialization { get; set; }

    /// <inheritdoc />
    public required global::Italbytz.AI.Evolutionary.PopulationManager.IPopulationManager PopulationManager { get; set; }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace SearchSpace { get; set; }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion[] StoppingCriteria { get; set; }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList Population => PopulationManager.Population;

    /// <inheritdoc />
    public int Generation
    {
        get => _ea.Generation;
        set => _ea.Generation = value;
    }

    /// <inheritdoc />
    public void InitPopulation()
    {
        Generation = 0;
        PopulationManager.InitPopulation(Initialization);
    }

    /// <inheritdoc />
    public Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> Run()
    {
        _ea = new EvolutionaryAlgorithm
        {
            FitnessFunction = FitnessFunction,
            SearchSpace = SearchSpace,
            Initialization = Initialization,
            PopulationManager = PopulationManager,
            StoppingCriteria = StoppingCriteria
        };
        _ea.AlgorithmGraph = new GenericGPGraph(
            SelectionForOperator,
            Mutations,
            Crossovers, SelectionForSurvival);
        return _ea.Run();
    }

    Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>
        global::Italbytz.AI.Evolutionary.IGeneticProgram.Run()
    {
        return RunAsEvolutionaryAsync();
    }

    private Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>
        RunAsEvolutionaryAsync() => Run();

}