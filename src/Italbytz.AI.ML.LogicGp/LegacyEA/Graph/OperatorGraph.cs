using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Graph;

internal class OperatorGraph
{
    protected Finish Finish { get; set; }
    protected Start Start { get; set; }
    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction FitnessFunction { get; set; }

    public Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> Process(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals)
    {
        return Start.Process(Task.FromResult(individuals), FitnessFunction);
    }

    public void Check()
    {
        List<GraphOperator> nodes =
        [
            Start
        ];
        while (nodes.Count > 0)
        {
            var node = nodes[0];
            nodes.RemoveAt(0);
            node.Check();
            foreach (var child in node.Children.Where(child =>
                         !nodes.Contains(child))) nodes.Add(child);
        }
    }
}