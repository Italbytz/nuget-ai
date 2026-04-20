# Umbrella World demo

The umbrella world demo is a public temporal-inference showcase in the `nuget-ai` demo host.

It uses the classic Hidden Markov Model from AIMA and compares filtering against smoothing over the same observation sequence.

## What the demo shows

- the umbrella/no-umbrella observation sequence day by day
- filtered posterior `P(Rain_t | e_1:t)` after each local observation
- smoothed posterior `P(Rain_t | e_1:T)` after the full sequence is known
- the effect of later evidence on earlier hidden-state beliefs

## Relevant package entry points

- `Italbytz.AI.Probability.HMM`
- `UmbrellaWorld`
- `ForwardBackwardAlgorithm`
- `HiddenMarkovModel`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/umbrella-world`

## Why this demo is useful

Umbrella world is a strong public showcase because it is:

- a canonical AIMA temporal-inference example
- small enough to visualize as a browser timeline
- useful for explaining the difference between filtering and smoothing
- grounded in reusable `Italbytz.AI.Probability.HMM` APIs instead of sample-only logic