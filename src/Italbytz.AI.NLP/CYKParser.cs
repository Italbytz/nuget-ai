using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.NLP;

namespace Italbytz.AI.NLP;

/// <summary>
/// CYK (Cocke-Younger-Kasami) probabilistic chart parser for PCFGs in CNF (AIMA3e p. 893).
/// table[A, i, j] = max log-probability of a parse of words[i..j] as non-terminal A.
/// Double.NegativeInfinity signals "no parse".
/// O(N³ × |G|) time where N = sentence length, |G| = number of grammar rules.
/// </summary>
public class CYKParser : ICFGParser
{
    public double[,,] Parse(IReadOnlyList<string> words, IGrammar grammar)
    {
        int n = words.Count;
        int ntCount = grammar.NonTerminals.Count;
        var table = new double[ntCount, n, n];

        // Initialise to -∞
        for (int a = 0; a < ntCount; a++)
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    table[a, i, j] = double.NegativeInfinity;

        // Lexical entries: table[A, i, i] for word i
        for (int i = 0; i < n; i++)
        {
            var word = words[i];
            foreach (var rule in grammar.Rules.Where(r => r.Right2 == null && r.Right1 == word))
            {
                int a = grammar.NonterminalIndex(rule.Left);
                table[a, i, i] = Math.Max(table[a, i, i], rule.LogProbability);
            }
            // Also look up via lexicon
            foreach (var nt in grammar.NonTerminals)
            {
                int a = grammar.NonterminalIndex(nt);
                foreach (var entry in grammar.Lexicon.GetEntries(word, nt))
                    table[a, i, i] = Math.Max(table[a, i, i], entry.LogProbability);
            }
        }

        // Fill table by increasing span length
        for (int span = 2; span <= n; span++)
        {
            for (int start = 0; start <= n - span; start++)
            {
                int end = start + span - 1;
                foreach (var rule in grammar.Rules.Where(r => r.Right2 != null))
                {
                    if (!grammar.NonTerminals.Contains(rule.Right1) ||
                        !grammar.NonTerminals.Contains(rule.Right2!))
                        continue;

                    int a = grammar.NonterminalIndex(rule.Left);
                    int b = grammar.NonterminalIndex(rule.Right1);
                    int c = grammar.NonterminalIndex(rule.Right2!);

                    for (int mid = start; mid < end; mid++)
                    {
                        double logP = rule.LogProbability + table[b, start, mid] + table[c, mid + 1, end];
                        if (logP > table[a, start, end])
                            table[a, start, end] = logP;
                    }
                }
            }
        }

        return table;
    }
}
