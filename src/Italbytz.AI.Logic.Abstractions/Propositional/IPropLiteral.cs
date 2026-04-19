namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional literal — a symbol optionally negated.</summary>
public interface IPropLiteral : IPropSentence
{
    string Symbol { get; }
    bool IsPositive { get; }
}
