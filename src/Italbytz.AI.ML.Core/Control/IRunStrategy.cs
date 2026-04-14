using System.Collections.Generic;
using Italbytz.AI.Evolutionary.Individuals;
using Microsoft.ML;

namespace Italbytz.AI.ML.Core.Control;

public interface IRunStrategy
{
    (IIndividual, IIndividualList) Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);
}