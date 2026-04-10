using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML;

public class Interpreter(ITransformer model)
{
    public ICanSaveModel ExternalizedModelParameters => model.GetModelParameters();
}
