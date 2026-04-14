namespace Italbytz.EA.StoppingCriterion;

/// <inheritdoc cref="global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion" />
internal class AdaptionsStoppingCriterion(
    IAdaptionCountProvider adaptionsProvider)
    : global::Italbytz.AI.Evolutionary.StoppingCriterion.IStoppingCriterion
{
    public int Limit { get; set; } = 100;

    /// <inheritdoc />
    public bool IsMet()
    {
        return adaptionsProvider.Adaptions >= Limit;
    }
}