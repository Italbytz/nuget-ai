using Italbytz.AI.Probability;
using Italbytz.AI.Probability.Bayes;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record BurglaryEvidenceConfiguration(
    bool? Earthquake,
    bool? Alarm,
    bool? JohnCalls,
    bool? MaryCalls);

internal sealed record BurglaryInferenceResult(
    string Key,
    string Name,
    string Summary,
    bool Approximate,
    double ProbabilityTrue,
    double ProbabilityFalse);

internal sealed record BurglaryNetworkNodeView(
    string Name,
    string[] Parents,
    string[] Children,
    string[] MarkovBlanket,
    string[][] ConditionalTable);

internal static class BurglaryNetworkDemoFactory
{
    public static BurglaryEvidenceConfiguration CreateDefaultEvidence()
    {
        return new BurglaryEvidenceConfiguration(null, null, true, true);
    }

    public static BurglaryEvidenceConfiguration CreateRandomEvidence(Random random)
    {
        return new BurglaryEvidenceConfiguration(
            RandomTriState(random),
            RandomTriState(random),
            RandomTriState(random),
            RandomTriState(random));
    }

    public static IReadOnlyList<BurglaryInferenceResult> BuildInferenceResults(BurglaryEvidenceConfiguration evidence, int seed)
    {
        var network = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidenceAssignments = ToEvidenceAssignments(evidence);

        return
        [
            CreateResult("Enumeration", "Enumeration ask", "Exact inference by enumerating all hidden assignments.", false, new EnumerationAsk(), query, evidenceAssignments, network),
            CreateResult("Elimination", "Elimination ask", "Exact inference via variable elimination and factor multiplication.", false, new EliminationAsk(), query, evidenceAssignments, network),
            CreateResult("LikelihoodWeighting", "Likelihood weighting", "Approximate inference through weighted sampling under the current evidence.", true, new LikelihoodWeighting(10000, seed), query, evidenceAssignments, network),
            CreateResult("GibbsAsk", "Gibbs ask", "Approximate inference through Gibbs sampling over non-evidence variables.", true, new GibbsAsk(10000, seed), query, evidenceAssignments, network)
        ];
    }

    public static IReadOnlyList<BurglaryNetworkNodeView> BuildNodeViews()
    {
        var network = BurglaryNetwork.Build();
        return network.Nodes
            .Select(BuildNodeView)
            .ToArray();
    }

    private static BurglaryInferenceResult CreateResult(
        string key,
        string name,
        string summary,
        bool approximate,
        IBayesInference algorithm,
        IRandomVariable query,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network)
    {
        var result = algorithm.Ask([query], evidence, network).Normalize();
        var probabilityTrue = result.ValueOf(new AssignmentProposition(query, true));

        return new BurglaryInferenceResult(
            key,
            name,
            summary,
            approximate,
            probabilityTrue,
            1.0 - probabilityTrue);
    }

    private static BurglaryNetworkNodeView BuildNodeView(IBayesNode node)
    {
        var rows = new List<string[]>();
        var parents = node.Parents.Select(parent => parent.RandomVariable).ToArray();

        EnumerateParentAssignments(parents, 0, [], assignment =>
        {
            var distribution = node.CpD.ConditionalOn(assignment.ToArray());
            var evidenceText = assignment.Count == 0
                ? "prior"
                : string.Join(", ", assignment.Select(parent => $"{parent.RandomVariable.Name}={FormatBoolean((bool)parent.Value)}"));
            rows.Add(
            [
                evidenceText,
                FormatProbability(distribution.ValueOf(new AssignmentProposition(node.RandomVariable, true))),
                FormatProbability(distribution.ValueOf(new AssignmentProposition(node.RandomVariable, false)))
            ]);
        });

        return new BurglaryNetworkNodeView(
            node.RandomVariable.Name,
            node.Parents.Select(parent => parent.RandomVariable.Name).ToArray(),
            node.Children.Select(child => child.RandomVariable.Name).ToArray(),
            node.MarkovBlanket.Select(blanketNode => blanketNode.RandomVariable.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            rows.ToArray());
    }

    private static IAssignmentProposition[] ToEvidenceAssignments(BurglaryEvidenceConfiguration evidence)
    {
        var assignments = new List<IAssignmentProposition>();

        AddIfPresent(assignments, BurglaryNetwork.Earthquake, evidence.Earthquake);
        AddIfPresent(assignments, BurglaryNetwork.Alarm, evidence.Alarm);
        AddIfPresent(assignments, BurglaryNetwork.JohnCalls, evidence.JohnCalls);
        AddIfPresent(assignments, BurglaryNetwork.MaryCalls, evidence.MaryCalls);

        return assignments.ToArray();
    }

    private static void AddIfPresent(List<IAssignmentProposition> assignments, IRandomVariable variable, bool? value)
    {
        if (value.HasValue)
        {
            assignments.Add(new AssignmentProposition(variable, value.Value));
        }
    }

    private static bool? RandomTriState(Random random)
    {
        return random.Next(3) switch
        {
            0 => null,
            1 => true,
            _ => false
        };
    }

    private static void EnumerateParentAssignments(
        IReadOnlyList<IRandomVariable> parents,
        int index,
        IReadOnlyList<IAssignmentProposition> current,
        Action<IReadOnlyList<IAssignmentProposition>> onAssignment)
    {
        if (index >= parents.Count)
        {
            onAssignment(current);
            return;
        }

        foreach (var value in parents[index].Domain.Values)
        {
            var next = current.Concat([new AssignmentProposition(parents[index], value)]).ToArray();
            EnumerateParentAssignments(parents, index + 1, next, onAssignment);
        }
    }

    public static string FormatProbability(double value)
    {
        return value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string FormatBoolean(bool value)
    {
        return value ? "true" : "false";
    }

    public static string FormatEvidence(bool? value)
    {
        return value switch
        {
            true => "true",
            false => "false",
            _ => "unknown"
        };
    }
}