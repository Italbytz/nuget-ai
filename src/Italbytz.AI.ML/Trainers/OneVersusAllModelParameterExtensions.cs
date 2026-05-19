using System.Collections.Immutable;
using System.Reflection;
using Microsoft.ML.Trainers;

namespace Italbytz.AI.ML.Trainers;

public static class OneVersusAllModelParameterExtensions
{
    public static PublicOneVersusAllModelParameters ToPublic(this OneVersusAllModelParameters modelParameters)
    {
        var publicModelParameters = new PublicOneVersusAllModelParameters();
        var subModelParamsProp = modelParameters.GetType()
            .GetProperty("SubModelParameters", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (subModelParamsProp?.GetValue(modelParameters) is IEnumerable<object> subModelParams)
        {
            publicModelParameters.SubModelParameters = [..subModelParams];
        }
        else
        {
            publicModelParameters.SubModelParameters = ImmutableArray<object>.Empty;
        }

        return publicModelParameters;
    }
}
