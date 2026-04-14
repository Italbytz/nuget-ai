using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Operator;

internal abstract class GraphOperator : global::Italbytz.AI.Evolutionary.Operator.IGraphOperator
{
    private readonly List<Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>>
        ParentTasks = [];

    public virtual int MaxParents { get; } = 1;
    public virtual int MaxChildren { get; } = 1;

    public List<GraphOperator> Children { get; } = [];
    public List<GraphOperator> Parents { get; } = [];

    IReadOnlyList<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator>
        global::Italbytz.AI.Evolutionary.Operator.IGraphOperator.Children
        => Children;

    IReadOnlyList<global::Italbytz.AI.Evolutionary.Operator.IGraphOperator>
        global::Italbytz.AI.Evolutionary.Operator.IGraphOperator.Parents
        => Parents;

    public void AddChildren(params global::Italbytz.AI.Evolutionary.Operator.IGraphOperator[] children)
    {
        foreach (var child in children)
        {
            if (child is not GraphOperator childOperator)
                throw new InvalidOperationException(
                    "Child operator is not legacy-compatible");

            Children.Add(childOperator);
            childOperator.Parents.Add(this);
        }
    }

    public void Check()
    {
        if (Children.Count > MaxChildren)
            throw new InvalidOperationException(
                $"Operator cannot have more than {MaxChildren} children.");
        if (Parents.Count > MaxParents)
            throw new InvalidOperationException(
                $"Operator cannot have more than {MaxParents} parents.");
    }

    public async Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>?
        Process(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        if (Parents.Count > 1)
        {
            ParentTasks.Add(individuals);
            if (ParentTasks.Count < Parents.Count) return null;

            var results = (await Task.WhenAll(ParentTasks)).ToList();

            var totalSize = results.Sum(r => r.Count);
            var combinedIndividuals = new ListBasedPopulation(totalSize);

            foreach (var parentIndividuals in results)
                combinedIndividuals.AddRange(parentIndividuals);

            individuals = Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(combinedIndividuals);
            ParentTasks.Clear();
        }

        var operationResult = Operate(individuals, fitnessFunction);

        if (Children.Count == 0)
            return await operationResult;

        if (Children.Count == 1)
            return await Children[0].Process(operationResult, fitnessFunction);

        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>? task = null;
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>? chosenTask = null;
        foreach (var child in Children)
        {
            task = child.Process(operationResult, fitnessFunction);
            if (task?.Result == null) continue;
            chosenTask = task;
        }

        return await chosenTask;
    }

    public abstract object Clone();

    Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>?
        global::Italbytz.AI.Evolutionary.Operator.IOperator.Process(
            Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
            global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        return Process(individuals, fitnessFunction);
    }

    public virtual Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>
        Operate(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        return individuals;
    }
}