# Two-Ply demo

The Two-Ply demo is a public adversarial-search showcase in the `nuget-ai` demo host.

It uses a compact depth-2 game tree to compare minimax and alpha-beta pruning under different action-order scenarios.

## What the demo shows

- deterministic two-level MAX/MIN game tree evaluation
- root-action utilities under perfect play
- node-expansion differences between minimax and alpha-beta
- impact of move ordering on pruning effectiveness

## Relevant package entry points

- `Italbytz.AI.Search`
- `Italbytz.AI.Search.Adversarial.MinimaxSearch`
- `Italbytz.AI.Search.Adversarial.AlphaBetaSearch`
- `Italbytz.AI.Search.Adversarial.IGame`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/two-ply`

## Why this demo is useful

The two-ply tree is small enough to reason about manually while still demonstrating a key engineering detail:

- decision quality remains unchanged between minimax and alpha-beta
- runtime cost is sensitive to branch ordering because pruning can cut off subtrees early
