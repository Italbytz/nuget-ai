using System.Collections.Generic;

namespace Italbytz.AI.Logic.Fol;

public class FolLexer
{
    public IReadOnlyList<FolToken> Tokenize(string input)
    {
        var tokens = new List<FolToken>();
        int index = 0;
        while (index < input.Length)
        {
            var current = input[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            switch (current)
            {
                case '(':
                    tokens.Add(new FolToken(FolTokenType.LeftParen, "("));
                    index++;
                    break;
                case ')':
                    tokens.Add(new FolToken(FolTokenType.RightParen, ")"));
                    index++;
                    break;
                case ',':
                    tokens.Add(new FolToken(FolTokenType.Comma, ","));
                    index++;
                    break;
                case '=':
                    if (index + 1 < input.Length && input[index + 1] == '>')
                    {
                        tokens.Add(new FolToken(FolTokenType.Implies, "=>"));
                        index += 2;
                    }
                    else
                    {
                        tokens.Add(new FolToken(FolTokenType.Equals, "="));
                        index++;
                    }
                    break;
                case '~':
                    tokens.Add(new FolToken(FolTokenType.Not, "~"));
                    index++;
                    break;
                case '&':
                    tokens.Add(new FolToken(FolTokenType.And, "&"));
                    index++;
                    break;
                case '|':
                    tokens.Add(new FolToken(FolTokenType.Or, "|"));
                    index++;
                    break;
                default:
                    if (char.IsLetter(current))
                    {
                        var start = index;
                        while (index < input.Length && (char.IsLetterOrDigit(input[index]) || input[index] == '_'))
                        {
                            index++;
                        }

                        var word = input[start..index];
                        tokens.Add(word switch
                        {
                            "FORALL" => new FolToken(FolTokenType.ForAll, word),
                            "EXISTS" => new FolToken(FolTokenType.Exists, word),
                            _ => new FolToken(FolTokenType.Identifier, word)
                        });
                        break;
                    }

                    throw new ArgumentException($"Unexpected character '{current}'.");
            }
        }

        tokens.Add(new FolToken(FolTokenType.End, string.Empty));
        return tokens;
    }
}