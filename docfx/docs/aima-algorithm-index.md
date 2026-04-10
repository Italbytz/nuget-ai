# AIMA algorithm index

This page preserves the most useful orientation table from the older AIMA-related documentation and remaps it to the consolidated `Italbytz.AI.*` package family.

It is intended as a quick bridge between **Artificial Intelligence: A Modern Approach (AIMA)** topics and the packages in this repository.

## Topic map

| Fig. | Page | Topic from the book | Current package family | Typical entry points |
| --- | ---: | --- | --- | --- |
| 2 | 34 | Environment | `Italbytz.AI.Agent.Abstractions` | `IEnvironment`, `IPercept` |
| 2.1 | 35 | Agent | `Italbytz.AI.Agent.Abstractions` / `Italbytz.AI.Agent` | `IAgent`, agent helpers |
| 3 | 66 | Problem | `Italbytz.AI.Search.Abstractions` | `IProblem`, search contracts |
| 3.10 | 79 | Node | `Italbytz.AI.Search.Abstractions` | `INode`, search state modeling |
| 4 | 147 | Online search problem | `Italbytz.AI.Search.Abstractions` | online-search related contracts where needed |
| 5.3 | 166 | Adversarial search | `Italbytz.AI.Search.Abstractions` / `Italbytz.AI.Search` | search-family extensions |
| 6 | 202 | Constraint satisfaction (CSP) | `Italbytz.AI.CSP.Abstractions` / `Italbytz.AI.CSP` | `ICSP`, `CSP`, `FlexibleBacktrackingSolver`, `TreeCspSolver`, `MinConflictsSolver` |
| 10.9 | 383 | Planning | `Italbytz.AI.Planning.Abstractions` / `Italbytz.AI.Planning` | `IPlanningProblem`, `PlanningProblem`, `PlanningProblemFactory` |
| 18.5 | 702 | Learner | `Italbytz.AI.Learning.Abstractions` / `Italbytz.AI.Learning` | `ILearner`, `DecisionTreeLearner`, `MajorityLearner` |
| 18.8 | 710 | Cross-validation | `Italbytz.AI.Learning.Abstractions` / `Italbytz.AI.Learning` | `ICrossValidation`, `CrossValidation` |

## Notes

- The original documentation pointed to separate `Ports` and `Adapters` repositories. The same content is now grouped by **domain** under `Italbytz.AI.*`.
- The official `aimacode` repositories (`aima-csharp`, `aima-python`, `aima-java`) remain useful references when you want broader textbook coverage.
- The current repo focuses on the slices that were actively migrated and verified during the refactoring wave.