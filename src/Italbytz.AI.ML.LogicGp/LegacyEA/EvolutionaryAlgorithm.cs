using System.Linq;
using System.Threading.Tasks;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Initialization;
using Italbytz.EA.PopulationManager;
using Italbytz.EA.SearchSpace;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA;

internal class EvolutionaryAlgorithm : global::Italbytz.AI.Evolutionary.StoppingCriterion.IGenerationProvider, IAdaptionCountProvider
{
    public required global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction FitnessFunction { get; set; }
    public required global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace SearchSpace { get; set; }
    public global::Italbytz.AI.Evolutionary.Initialization.IInitialization? Initialization { get; set; }

    public global::Italbytz.AI.Evolutionary.PopulationManager.IPopulationManager PopulationManager { get; set; } =
        new DefaultPopulationManager();

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList Population => PopulationManager.Population;

    public global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion[] StoppingCriteria { get; set; }
    public OperatorGraph AlgorithmGraph { get; set; }
    public int Generation { get; set; }

    public async Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> Run()
    {
        AlgorithmGraph.Check();
        AlgorithmGraph.FitnessFunction = FitnessFunction;
        Generation = 0;
        PopulationManager.InitPopulation(Initialization);
        var stop = false;
        while (!stop)
        {
            stop = StoppingCriteria.Any(sc => sc.IsMet());
            if (stop) continue;
            var newPopulation = await AlgorithmGraph.Process(Population);
            Generation++;
            foreach (var individual in newPopulation)
                individual.Generation = Generation;
            PopulationManager.Population = newPopulation;
            /*Console.Out.WriteLine(((DefaultPopulationManager)PopulationManager)
                .GetPopulationInfo());*/
        }

        return PopulationManager.Population;
    }

    public int Adaptions { get; set; }
}