using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// A FOL inference engine: answers a query against a knowledge base by returning
/// all substitutions θ such that the query is entailed given θ.
/// </summary>
public interface IFolInference
{
    IEnumerable<ISubstitution> Ask(IFolKnowledgeBase kb, ISentence query);
}
