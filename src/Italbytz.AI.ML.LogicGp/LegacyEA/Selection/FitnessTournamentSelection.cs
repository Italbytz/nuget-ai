namespace Italbytz.EA.Selection;

internal class FitnessTournamentSelection : TournamentSelection
{
    public override bool UseDomination { get; } = false;
}