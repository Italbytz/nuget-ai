namespace Italbytz.EA.StoppingCriterion;

/// <inheritdoc cref="global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion" />
internal class GenerationStoppingCriterion(
    global::Italbytz.AI.Evolutionary.StoppingCriterion.IGenerationProvider generationProvider)
    : global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion
{
    public int Limit { get; set; } = 100;

    /// <inheritdoc />
    public bool IsMet()
    {
        return generationProvider.Generation >= Limit;
    }
}