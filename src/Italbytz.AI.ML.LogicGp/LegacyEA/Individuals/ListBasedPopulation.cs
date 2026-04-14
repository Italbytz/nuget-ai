using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividualList" />
public class ListBasedPopulation : global::Italbytz.AI.Evolutionary.Individuals.IIndividualList,
    IIndividualList
{
    private readonly List<global::Italbytz.AI.Evolutionary.Individuals.IIndividual>
        _individuals = [];

    public ListBasedPopulation()
    {
    }

    public ListBasedPopulation(int capacity)
    {
        _individuals = new List<global::Italbytz.AI.Evolutionary.Individuals.IIndividual>(capacity);
    }

    public void Freeze()
    {
        foreach (var individual in _individuals)
            (individual.Genotype as global::Italbytz.AI.Evolutionary.Individuals.IFreezable)?.Freeze();
    }

    public int Count => _individuals.Count;
    public bool IsReadOnly => false;

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividual this[int index]
    {
        get => _individuals[index];
        set => _individuals[index] = value;
    }

    IIndividual IIndividualList.this[int index] => AsLegacyIndividual(_individuals[index]);

    /// <inheritdoc />
    public void Add(global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        _individuals.Add(individual);
    }

    void IIndividualList.Add(IIndividual individual)
    {
        Add(individual);
    }

    public void RemoveAt(int index)
    {
        _individuals.RemoveAt(index);
    }

    public void Clear()
    {
        _individuals.Clear();
    }

    public bool Contains(global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        return _individuals.Contains(individual);
    }

    public void CopyTo(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual[] array,
        int arrayIndex)
    {
        _individuals.CopyTo(array, arrayIndex);
    }

    public int IndexOf(global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        return _individuals.IndexOf(individual);
    }

    public void Insert(int index,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        _individuals.Insert(index, individual);
    }

    public bool Remove(global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        return _individuals.Remove(individual);
    }

    bool IIndividualList.Remove(IIndividual individual)
    {
        return Remove(individual);
    }

    /// <inheritdoc />
    public global::Italbytz.AI.Evolutionary.Individuals.IIndividual GetRandomIndividual()
    {
        return _individuals[
            ThreadSafeRandomNetCore.Shared.Next(_individuals.Count)];
    }

    IIndividual IIndividualList.GetRandomIndividual()
    {
        return AsLegacyIndividual(GetRandomIndividual());
    }

    public void AddRange(
        IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> individuals)
    {
        _individuals.AddRange(individuals);
    }

    void IIndividualList.AddRange(IEnumerable<IIndividual> individuals)
    {
        foreach (var individual in individuals)
            _individuals.Add(individual);
    }

    void IIndividualList.Add(IIndividualList individuals)
    {
        foreach (var individual in individuals)
            _individuals.Add(individual);
    }

    /// <inheritdoc />
    public IEnumerator<global::Italbytz.AI.Evolutionary.Individuals.IIndividual>
        GetEnumerator()
    {
        return _individuals.GetEnumerator();
    }

    IEnumerator<IIndividual> IEnumerable<IIndividual>.GetEnumerator()
    {
        foreach (var individual in _individuals)
            yield return AsLegacyIndividual(individual);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    List<IIndividual> IIndividualList.ToList()
    {
        var result = new List<IIndividual>(_individuals.Count);
        foreach (var individual in _individuals)
            result.Add(AsLegacyIndividual(individual));
        return result;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join("\n", _individuals);
    }

    private static IIndividual AsLegacyIndividual(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        return individual as IIndividual
               ?? throw new System.InvalidCastException(
                   "Expected a LegacyEA IIndividual implementation.");
    }
}