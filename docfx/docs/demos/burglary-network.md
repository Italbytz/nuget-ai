# Burglary network demo

The burglary network demo is a public probability showcase in the `nuget-ai` demo host.

It uses the classic Burglary-Alarm Bayesian network from AIMA and compares exact and approximate inference on the same evidence configuration.

## What the demo shows

- exact inference with `EnumerationAsk`
- exact inference with `EliminationAsk`
- approximate inference with `LikelihoodWeighting`
- approximate inference with `GibbsAsk`
- live evidence changes for earthquake, alarm, and call variables

## Relevant package entry points

- `Italbytz.AI.Probability`
- `BurglaryNetwork`
- `EnumerationAsk`
- `EliminationAsk`
- `LikelihoodWeighting`
- `GibbsAsk`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/burglary-network`

## Why this demo is useful

The burglary network is a strong public showcase because it is:

- a canonical AIMA Bayesian-network example
- small enough to inspect both the graph and the CPTs in one browser view
- useful for comparing exact and approximate inference directly
- grounded in reusable `Italbytz.AI.Probability` APIs instead of sample-only code