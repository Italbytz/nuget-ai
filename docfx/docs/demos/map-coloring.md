# Map coloring demo

The map coloring demo is a public CSP showcase in the `nuget-ai` demo host.

It uses the classic Australia map example from AIMA and compares search-tree based solving with repair-based solving on the same constraint graph.

## What the demo shows

- classic backtracking
- heuristic backtracking with MRV/DEG and LCV
- min-conflicts repair
- randomized tie orders and repair trajectories
- region assignments and visible neighbor conflicts

## Relevant package entry points

- `Italbytz.AI.CSP`
- `MapCSP`
- `FlexibleBacktrackingSolver`
- `MinConflictsSolver`
- `CspHeuristics`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/map-coloring`

## Why this demo is useful

Map coloring is a strong public showcase because it is:

- a canonical AIMA CSP example
- compact enough for a browser-based step trace
- suitable for comparing constructive search and repair-based local search
- directly tied to reusable `Italbytz.AI.CSP` abstractions and solvers