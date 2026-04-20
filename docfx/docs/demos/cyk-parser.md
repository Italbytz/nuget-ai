# CYK parser demo

The CYK parser demo is a public NLP showcase in the `nuget-ai` demo host.

It uses the probabilistic Cocke-Younger-Kasami parser on a small CNF grammar and reveals the chart cell by cell.

## What the demo shows

- lexical base cases on span length 1
- larger chart cells filled by increasing substring span
- surviving non-terminals and their probabilities per substring
- a direct comparison between parsable and non-parsable example sentences

## Relevant package entry points

- `Italbytz.AI.NLP`
- `CYKParser`
- `Grammar`
- `GrammarRule`
- `Lexicon`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/cyk-parser`

## Why this demo is useful

The CYK chart is a strong public showcase because it is:

- a textbook dynamic-programming parser from AIMA
- small enough to reveal step by step in a browser
- useful for explaining how local lexical evidence grows into a full parse
- grounded in reusable `Italbytz.AI.NLP` parsing types instead of sample-only code