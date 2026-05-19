using System.Collections.Immutable;
using Microsoft.ML;

namespace Italbytz.AI.ML.Trainers;

/// <summary>
/// Public shim for One-Versus-All model parameters that exposes inner sub-models.
/// </summary>
public class PublicOneVersusAllModelParameters : ICanSaveModel
{
    public ImmutableArray<object> SubModelParameters { get; set; }

    public void Save(ModelSaveContext ctx)
    {
        throw new NotImplementedException();
    }
}
