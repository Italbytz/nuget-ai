using Italbytz.AI.Evolutionary.Operator;
using Italbytz.AI.Evolutionary.SearchSpace;

namespace Italbytz.AI.Evolutionary.Initialization;

public interface IInitialization : IOperator
{
    ISearchSpace SearchSpace { get; set; }
}