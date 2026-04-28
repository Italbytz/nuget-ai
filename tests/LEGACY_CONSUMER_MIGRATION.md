# Legacy Consumer Migration Map

This file documents the replacement of the removed standalone consumer repositories `csharp-mstest-ai` and `csharp-mstest-logicgp`.

## csharp-mstest-ai

- old location: `artifacts/consumers/production/csharp-mstest-ai`
- replacement inside `nuget-ai`: `samples/legacy-aima-mstest/`
- active maintained coverage in `nuget-ai/tests/Italbytz.AI.Tests/`

Already absorbed into active `nuget-ai` tests:

- Romania map and search-agent regressions in `RomaniaMapSearchIntegrationTests.cs` and `SearchAgentAndBreadthFirstSearchTests.cs`
- adversarial-search regressions for the old two-ply sample in `AdversarialSearchTests.cs`
- informed-search regressions for greedy best-first and recursive best-first search in `InformedSearchLegacyRegressionTests.cs`
- CSP cases from the old map/tree suites in `CspIntegrationTests.cs` plus legacy assignment/constraint-domain regressions in `CspLegacyRegressionTests.cs`
- planning coverage in `PlanningIntegrationTests.cs` plus parser regression coverage in `PlanningLegacyRegressionTests.cs`
- learning coverage for restaurant and decision-tree examples in `LearningIntegrationTests.cs`

The copied sample snapshot preserves the remaining older AIMA-style example material such as `NQueens` and `TwoPly` without keeping a separate external repository alive.

`LPSolver` now exists again in the current `nuget-ai` source tree as a managed ALGLIB-backed port with active regression coverage in `LinearProgrammingRegressionTests.cs`, including model-based LP/ILP solving, `lp_solve` text parsing, `MPS_FIXED` parsing and file-based solve entry points.

Still not migrated into the active `nuget-ai/tests/Italbytz.AI.Tests/` suite are only the less-used MPS variants beyond `MPS_FIXED` (`MPS_FREE`, `MPS_IBM`, `MPS_NEGOBJCONST`). Those remain unsupported until they are needed by an active consumer.

## csharp-mstest-logicgp

- old location: `artifacts/consumers/production/csharp-mstest-logicgp`
- replacement inside `nuget-ai`: `benchmarks/legacy-logicgp-regression/`
- active maintained compact coverage in `nuget-ai/tests/Italbytz.AI.Tests/MlIntegrationTests.cs`

Already absorbed into active `nuget-ai` tests:

- compact starter cases for Iris, HeartDisease binary/multiclass, BalanceScale, WineQuality, BreastCancerWisconsinDiagnostic, Lenses, CarEvaluation, SolarFlare, NPHA, Restaurant and further dataset coverage in `MlIntegrationTests.cs`

The copied benchmark snapshot preserves the larger dataset-driven and simulation-heavy logicGP material so that it stays available inside `nuget-ai` while the standalone consumer repository can be removed.