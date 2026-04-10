using System.Collections.Generic;

namespace Italbytz.AI.Evolutionary.Individuals;

public class ListBasedPopulation : List<IIndividual>, IIndividualList
{
    public ListBasedPopulation()
    {
    }

    public ListBasedPopulation(IEnumerable<IIndividual> individuals) : base(individuals)
    {
    }
}
