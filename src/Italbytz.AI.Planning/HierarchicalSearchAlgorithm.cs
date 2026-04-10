using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Planning;

public class HierarchicalSearchAlgorithm
{
    public IEnumerable<IActionSchema>? HierarchicalSearch(IPlanningProblem problem)
    {
        var frontier = new LinkedList<List<IActionSchema>>();
        frontier.AddLast(new List<IActionSchema> { PlanningProblemFactory.GetHlaAct(problem) });

        while (true)
        {
            if (frontier.Count == 0)
            {
                return null;
            }

            var plan = frontier.First!.Value;
            frontier.RemoveFirst();

            var index = 0;
            IActionSchema? hla;
            while (index < plan.Count && (hla = plan[index]) is not HighLevelAction)
            {
                index++;
            }

            hla = index < plan.Count ? plan[index] : null;
            var prefix = new List<IActionSchema>();
            var suffix = new List<IActionSchema>();
            for (var i = 0; i < index; i++)
            {
                prefix.Add(plan[i]);
            }
            for (var i = index + 1; i < plan.Count; i++)
            {
                suffix.Add(plan[i]);
            }

            var outcome = problem.InitialState.Result(prefix);
            if (hla is null)
            {
                if (problem.Goal.All(goal => outcome.Fluents.Contains(goal)))
                {
                    return plan;
                }
            }
            else
            {
                foreach (var refinement in Refinements(hla, outcome))
                {
                    var newPlan = new List<IActionSchema>();
                    newPlan.AddRange(prefix);
                    newPlan.AddRange(refinement);
                    newPlan.AddRange(suffix);
                    frontier.AddLast(newPlan);
                }
            }
        }
    }

    private static IEnumerable<IEnumerable<IActionSchema>> Refinements(IActionSchema hla, IState outcome)
    {
        var refinements = new List<List<IActionSchema>>();
        foreach (var refinement in ((HighLevelAction)hla).Refinements)
        {
            if (refinement.Count > 0)
            {
                if (outcome.IsApplicable(refinement[0]))
                {
                    refinements.Add(refinement);
                }
            }
            else
            {
                refinements.Add(refinement);
            }
        }

        return refinements;
    }
}
