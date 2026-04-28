using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.AI.Evolutionary.Fitness;
using Italbytz.AI.Evolutionary.Individuals;
using Italbytz.AI.Evolutionary.Initialization;
using Italbytz.AI.Evolutionary.Operator;
using Italbytz.AI.Evolutionary.PopulationManager;
using Italbytz.AI.Evolutionary.SearchSpace;
using Italbytz.AI.Evolutionary.StoppingCriterion;

namespace Italbytz.AI.Evolutionary;

public interface IGeneticProgram
{
    IReadOnlyList<IGraphOperator> Mutations { get; set; }

    IReadOnlyList<IGraphOperator> Crossovers { get; set; }

    IIndividualList Population { get; }

    IInitialization Initialization { get; set; }

    IPopulationManager PopulationManager { get; set; }

    ISearchSpace SearchSpace { get; set; }

    IGraphOperator SelectionForOperator { get; set; }

    IGraphOperator SelectionForSurvival { get; set; }

    IReadOnlyList<IStoppingCriterion> StoppingCriteria { get; set; }

    int Generation { get; set; }

    IFitnessFunction FitnessFunction { get; set; }

    Task<(IIndividual, IIndividualList)> Run();
}