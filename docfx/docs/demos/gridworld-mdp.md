# GridWorld MDP demo

The GridWorld MDP demo is a public decision-making showcase in the `nuget-ai` demo host.

It uses the classic stochastic 4x3 grid world from AIMA and compares value iteration against policy iteration on the same Markov Decision Process.

## What the demo shows

- final utility estimates for every reachable state
- greedy policy arrows induced by the solved utility landscape
- action-by-action expected successor utility for the currently inspected state
- the effect of stochastic transitions with 0.8 intended movement and 0.1 side slips

## Relevant package entry points

- `Italbytz.AI.Probability.MDP`
- `GridWorldMdp`
- `ValueIteration`
- `PolicyIteration`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/gridworld-mdp`

## Why this demo is useful

The grid world is a strong public showcase because it is:

- a canonical AIMA MDP example
- compact enough for a browser-based utility heatmap and policy display
- useful for comparing two standard dynamic-programming solvers on the same model
- grounded in reusable `Italbytz.AI.Probability.MDP` APIs instead of sample-only code