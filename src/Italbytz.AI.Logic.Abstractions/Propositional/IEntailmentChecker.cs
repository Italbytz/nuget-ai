namespace Italbytz.AI.Logic.Propositional;

/// <summary>Checks whether a knowledge base entails a propositional query symbol.</summary>
public interface IEntailmentChecker
{
    bool IsEntailed(IPropKnowledgeBase kb, string query);
}
