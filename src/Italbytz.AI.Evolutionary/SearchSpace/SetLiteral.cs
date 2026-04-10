using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class SetLiteral<TCategory> : ISetLiteral<TCategory>
    where TCategory : notnull
{
    private readonly bool[] _bitSet;
    private readonly Dictionary<TCategory, int> _categoryIndexMap;

    [JsonConstructor]
    public SetLiteral(int feature, IList<TCategory> categories, int set, SetLiteralType literalType = SetLiteralType.Rudell)
    {
        Feature = feature;
        Categories = categories;
        Set = set;
        LiteralType = literalType;
        Label = $"F{feature}";
        _bitSet = new bool[Categories.Count];
        _categoryIndexMap = new Dictionary<TCategory, int>(Categories.Count);

        for (var i = 0; i < Categories.Count; i++)
        {
            _bitSet[i] = (set & (1 << i)) != 0;
            _categoryIndexMap[Categories[i]] = i;
        }
    }

    [JsonInclude] public IList<TCategory> Categories { get; }

    [JsonInclude] public int Feature { get; }

    [JsonInclude] public SetLiteralType LiteralType { get; }

    [JsonInclude] public int Set { get; }

    [JsonIgnore] public string Label { get; }

    public int CompareTo(ILiteral<TCategory>? other)
    {
        if (other is null) return 1;
        if (other is not SetLiteral<TCategory> literal) return -1;

        var labelComparison = string.Compare(Label, literal.Label, StringComparison.Ordinal);
        return labelComparison != 0 ? labelComparison : Set.CompareTo(literal.Set);
    }

    public bool Evaluate(TCategory[] input)
    {
        var value = input[Feature];
        return _categoryIndexMap.TryGetValue(value, out var index) && index < _bitSet.Length && _bitSet[index];
    }

    public override bool Equals(object? obj)
    {
        return obj is SetLiteral<TCategory> other && other.Label == Label && other.Set == Set && other.LiteralType == LiteralType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Set, LiteralType);
    }

    public override string ToString()
    {
        return LiteralType switch
        {
            SetLiteralType.Dussault => ToDussaultString(),
            SetLiteralType.Rudell => ToRudellString(),
            SetLiteralType.Su => ToSuString(),
            SetLiteralType.LessGreater => ToLessGreaterString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string ToRudellString()
    {
        var values = Categories.Where((_, index) => _bitSet[index]).ToList();
        return $"({Label} ∈ {{{string.Join(",", values)}}})";
    }

    private string ToDussaultString()
    {
        var positives = _bitSet.Count(bit => bit);
        if (positives == 1)
        {
            return $"({Label} = {Categories[Array.IndexOf(_bitSet, true)]})";
        }

        if (positives == _bitSet.Length - 1)
        {
            return $"({Label} ≠ {Categories[Array.IndexOf(_bitSet, false)]})";
        }

        throw new ArgumentException("Dussault literals must have exactly one or all but one bit set.");
    }

    private string ToSuString()
    {
        var firstPositive = Array.IndexOf(_bitSet, true);
        var lastPositive = Array.LastIndexOf(_bitSet, true);
        var firstNegative = Array.IndexOf(_bitSet, false);
        var lastNegative = Array.LastIndexOf(_bitSet, false);

        if (firstPositive == -1 || firstNegative == -1)
        {
            throw new ArgumentException("Su literals require both positive and negative values.");
        }

        var hasHole = Enumerable.Range(firstPositive, lastPositive - firstPositive + 1).Any(index => !_bitSet[index]);
        return hasHole
            ? $"({Label} ∉ [{Categories[firstNegative]},{Categories[lastNegative]}])"
            : $"({Label} ∈ [{Categories[firstPositive]},{Categories[lastPositive]}])";
    }

    private string ToLessGreaterString()
    {
        var builder = new StringBuilder();
        if (_bitSet[0])
        {
            var index = Array.IndexOf(_bitSet, false);
            builder.Append($"({Label} < {Categories[index]})");
        }
        else
        {
            var index = Array.IndexOf(_bitSet, true);
            builder.Append($"({Label} > {Categories[index - 1]})");
        }

        return builder.ToString();
    }
}
