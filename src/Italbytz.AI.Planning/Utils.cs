using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public static class Utils
{
    public static IList<ILiteral> Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<ILiteral>();
        }

        var normalized = text.Replace(" ", string.Empty);
        var tokens = normalized.Split('^', System.StringSplitOptions.RemoveEmptyEntries);
        var literals = new List<ILiteral>();
        foreach (var token in tokens)
        {
            var terms = token.Split('(', ')', ',');
            var literalTerms = new List<ITerm>();
            var negated = false;
            for (var i = 1; i < terms.Length; i++)
            {
                var termString = terms[i];
                if (string.IsNullOrEmpty(termString))
                {
                    continue;
                }

                ITerm term = termString == termString.ToLowerInvariant()
                    ? new Variable(termString)
                    : new Constant(termString);
                literalTerms.Add(term);
            }

            var name = terms[0];
            if (name.StartsWith('~'))
            {
                negated = true;
                name = name[1..];
            }

            var predicate = new Predicate(name, literalTerms);
            literals.Add(new Literal(predicate, negated));
        }

        return literals;
    }
}
