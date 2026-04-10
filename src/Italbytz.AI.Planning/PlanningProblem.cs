using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public class PlanningProblem : IPlanningProblem
{
    private readonly ISet<ActionSchema> _actionSchemas;
    private IList<IActionSchema>? _propositionalisedActionSchemas;

    public PlanningProblem(State initialState, IList<ILiteral> goal, ISet<ActionSchema> actionSchemas)
    {
        InitialState = initialState;
        Goal = goal;
        _actionSchemas = actionSchemas;
    }

    public PlanningProblem(State initialState, IList<ILiteral> goal, params ActionSchema[] actions)
        : this(initialState, goal, new HashSet<ActionSchema>(actions))
    {
    }

    public IList<ILiteral> Goal { get; }

    public IState InitialState { get; }

    public IEnumerable<IActionSchema> GetPropositionalisedActions()
    {
        if (_propositionalisedActionSchemas is null)
        {
            var problemConstants = GetProblemConstants();
            _propositionalisedActionSchemas = new List<IActionSchema>();
            foreach (var actionSchema in _actionSchemas)
            {
                if (actionSchema.Variables.Count == 0)
                {
                    _propositionalisedActionSchemas.Add(actionSchema);
                    continue;
                }

                foreach (var constants in GenerateAssignments(problemConstants, actionSchema.Variables.Count))
                {
                    _propositionalisedActionSchemas.Add(actionSchema.GetActionBySubstitution(constants));
                }
            }
        }

        return _propositionalisedActionSchemas;
    }

    private IList<IConstant> GetProblemConstants()
    {
        var constants = new List<IConstant>();

        void AddConstants(IEnumerable<ILiteral> literals)
        {
            foreach (var literal in literals)
            {
                foreach (var term in literal.Atom.Args)
                {
                    if (term is IConstant constant && !constants.Contains(constant))
                    {
                        constants.Add(constant);
                    }
                }
            }
        }

        AddConstants(InitialState.Fluents);
        AddConstants(Goal);
        foreach (var actionSchema in _actionSchemas)
        {
            foreach (var constant in actionSchema.GetConstants())
            {
                if (!constants.Contains(constant))
                {
                    constants.Add(constant);
                }
            }
        }

        return constants;
    }

    private static IEnumerable<IReadOnlyList<IConstant>> GenerateAssignments(IList<IConstant> constants, int length)
    {
        if (length == 0)
        {
            yield return [];
            yield break;
        }

        foreach (var constant in constants)
        {
            foreach (var suffix in GenerateAssignments(constants, length - 1))
            {
                var assignment = new List<IConstant> { constant };
                assignment.AddRange(suffix);
                yield return assignment;
            }
        }
    }
}
