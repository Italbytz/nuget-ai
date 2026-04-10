using System.Collections.Generic;

namespace Italbytz.AI.Planning;

public static class PlanningProblemFactory
{
    public static IPlanningProblem GoHomeToSfoProblem()
    {
        var initialState = new State("At(Home)");
        var goal = Utils.Parse("At(SFO)");
        var driveAction = new ActionSchema("Drive", null, "At(Home)", "~At(Home)^At(SFOLongTermParking)");
        var shuttleAction = new ActionSchema("Shuttle", null, "At(SFOLongTermParking)", "~At(SFOLongTermParking)^At(SFO)");
        var taxiAction = new ActionSchema("Taxi", null, "At(Home)", "~At(Home)^At(SFO)");
        return new PlanningProblem(initialState, goal, driveAction, shuttleAction, taxiAction);
    }

    public static HighLevelAction GetHlaAct(IPlanningProblem problem)
    {
        var refinements = new List<List<IActionSchema>>();
        var act = new HighLevelAction("Act", null, string.Empty, string.Empty, refinements);
        foreach (var primitiveAction in problem.GetPropositionalisedActions())
        {
            act.Refinements.Add(new List<IActionSchema> { primitiveAction, act });
        }

        act.Refinements.Add(new List<IActionSchema>());
        return act;
    }
}
