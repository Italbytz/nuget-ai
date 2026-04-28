using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;
using Constant = Italbytz.AI.Planning.Fol.Constant;
using ConnectedSentence = Italbytz.AI.Planning.Fol.ConnectedSentence;
using Function = Italbytz.AI.Planning.Fol.Function;
using NotSentence = Italbytz.AI.Planning.Fol.NotSentence;
using Predicate = Italbytz.AI.Planning.Fol.Predicate;
using QuantifiedSentence = Italbytz.AI.Planning.Fol.QuantifiedSentence;
using TermEquality = Italbytz.AI.Planning.Fol.TermEquality;
using Variable = Italbytz.AI.Planning.Fol.Variable;

namespace Italbytz.AI.Logic.Fol;

public class FolParser
{
    private readonly FolDomain _domain;
    private readonly FolLexer _lexer = new();
    private IReadOnlyList<FolToken> _tokens = Array.Empty<FolToken>();
    private int _position;

    public FolParser(FolDomain? domain = null)
    {
        _domain = domain ?? new FolDomain();
    }

    public ISentence Parse(string input)
    {
        _tokens = _lexer.Tokenize(input);
        _position = 0;
        var sentence = ParseImplication();
        Expect(FolTokenType.End);
        return sentence;
    }

    private ISentence ParseImplication()
    {
        var left = ParseOr();
        while (Match(FolTokenType.Implies))
        {
            var right = ParseImplication();
            left = new ConnectedSentence("=>", left, right);
        }

        return left;
    }

    private ISentence ParseOr()
    {
        var left = ParseAnd();
        while (Match(FolTokenType.Or))
        {
            left = new ConnectedSentence("|", left, ParseAnd());
        }

        return left;
    }

    private ISentence ParseAnd()
    {
        var left = ParseUnary();
        while (Match(FolTokenType.And))
        {
            left = new ConnectedSentence("&", left, ParseUnary());
        }

        return left;
    }

    private ISentence ParseUnary()
    {
        if (Match(FolTokenType.Not))
        {
            return new NotSentence(ParseUnary());
        }

        if (Match(FolTokenType.ForAll))
        {
            return ParseQuantified("FORALL");
        }

        if (Match(FolTokenType.Exists))
        {
            return ParseQuantified("EXISTS");
        }

        if (Match(FolTokenType.LeftParen))
        {
            var sentence = ParseImplication();
            Expect(FolTokenType.RightParen);
            return sentence;
        }

        return ParseAtomic();
    }

    private ISentence ParseQuantified(string quantifier)
    {
        var variables = new List<IVariable> { new Variable(Expect(FolTokenType.Identifier).Lexeme) };
        while (Match(FolTokenType.Comma))
        {
            variables.Add(new Variable(Expect(FolTokenType.Identifier).Lexeme));
        }

        var sentence = ParseUnary();
        return new QuantifiedSentence(quantifier, variables, sentence);
    }

    private ISentence ParseAtomic()
    {
        var identifier = Expect(FolTokenType.Identifier).Lexeme;
        if (Check(FolTokenType.LeftParen))
        {
            Advance();
            var args = ParseTermList();
            Expect(FolTokenType.RightParen);

            if (_domain.Functions.Contains(identifier) && !_domain.Predicates.Contains(identifier))
            {
                var left = new Function(identifier, args);
                Expect(FolTokenType.Equals);
                return new TermEquality(left, ParseTerm());
            }

            return new Predicate(identifier, args);
        }

        var leftTerm = ClassifySimpleTerm(identifier);
        if (Match(FolTokenType.Equals))
        {
            return new TermEquality(leftTerm, ParseTerm());
        }

        return new Predicate(identifier, new List<ITerm>());
    }

    private List<ITerm> ParseTermList()
    {
        var terms = new List<ITerm>();
        if (Check(FolTokenType.RightParen))
        {
            return terms;
        }

        terms.Add(ParseTerm());
        while (Match(FolTokenType.Comma))
        {
            terms.Add(ParseTerm());
        }

        return terms;
    }

    private ITerm ParseTerm()
    {
        var identifier = Expect(FolTokenType.Identifier).Lexeme;
        if (Match(FolTokenType.LeftParen))
        {
            var args = ParseTermList();
            Expect(FolTokenType.RightParen);
            return new Function(identifier, args);
        }

        return ClassifySimpleTerm(identifier);
    }

    private ITerm ClassifySimpleTerm(string identifier)
    {
        if (_domain.Constants.Contains(identifier))
        {
            return new Constant(identifier);
        }

        if (char.IsLower(identifier[0]))
        {
            return new Variable(identifier);
        }

        return new Constant(identifier);
    }

    private bool Match(FolTokenType type)
    {
        if (!Check(type))
        {
            return false;
        }

        Advance();
        return true;
    }

    private bool Check(FolTokenType type) => Current.Type == type;

    private FolToken Advance()
    {
        var current = Current;
        _position++;
        return current;
    }

    private FolToken Expect(FolTokenType type)
    {
        if (!Check(type))
        {
            throw new ArgumentException($"Expected token {type} but found {Current.Type}.");
        }

        return Advance();
    }

    private FolToken Current => _tokens[_position];
}