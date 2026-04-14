using System;
using System.Threading.Tasks;
using Italbytz.AI.Evolutionary.Fitness;
using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.Operator;

public interface IOperator : ICloneable
{
    Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction);
}