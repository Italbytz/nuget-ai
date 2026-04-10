using System.Globalization;
using Italbytz.AI.Learning;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Learners;
using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.Trainers;

public abstract class DecisionTreeTrainer<TOutput> : CustomClassificationTrainer<TOutput>
    where TOutput : class, new()
{
    protected readonly ILearner Learner = new DecisionTreeLearner();
    protected IDataExcerpt? DataExcerpt;
    protected IDataSetSpecification? Spec;

    protected override void PrepareForFit(IDataView input)
    {
        DataExcerpt = input.GetDataExcerpt();
        Spec = DataExcerpt.GetDataSetSpecification();
        var dataSet = DataExcerpt.GetDataSet(Spec);
        Learner.Train(dataSet);
    }

    protected override void Map(ClassificationInput input, TOutput output)
    {
        var example = ToExample(input);
        var prediction = Learner.Predict(example);
        switch (output)
        {
            case IBinaryClassificationOutput binaryOutput:
                binaryOutput.PredictedLabel = uint.Parse(prediction, CultureInfo.InvariantCulture);
                binaryOutput.Score = prediction == "1" ? 0f : 1f;
                binaryOutput.Probability = prediction == "1" ? 0f : 1f;
                break;
            case IMulticlassClassificationOutput multiclassOutput:
            {
                var classes = DataExcerpt?.UniqueLabelValues.Length ?? 0;
                var predictedLabel = uint.Parse(prediction, CultureInfo.InvariantCulture);
                multiclassOutput.PredictedLabel = predictedLabel;
                var scores = new float[classes];
                scores[Math.Max(0, predictedLabel - 1)] = 1f;
                multiclassOutput.Score = new VBuffer<float>(scores.Length, scores);
                multiclassOutput.Probability = new VBuffer<float>(scores.Length, scores);
                break;
            }
            default:
                throw new ArgumentException("Unsupported output type.", nameof(output));
        }
    }

    private IExample ToExample<TSrc>(TSrc src) where TSrc : class, new()
    {
        if (src is not ICustomMappingInput input || DataExcerpt == null || Spec == null)
        {
            throw new ArgumentException("The input is not compatible with the decision-tree trainer.", nameof(src));
        }

        Dictionary<string, IAttribute> attributes = new();
        for (var index = 0; index < DataExcerpt.FeatureNames.Length; index++)
        {
            var featureName = DataExcerpt.FeatureNames[index];
            attributes[featureName] = new StringAttribute(
                input.Features[index].ToString(CultureInfo.InvariantCulture),
                Spec.GetAttributeSpecFor(featureName));
        }

        var targetAttribute = new StringAttribute("1", Spec.GetAttributeSpecFor(DefaultColumnNames.Label));
        attributes[DefaultColumnNames.Label] = targetAttribute;
        return new Example(attributes, targetAttribute);
    }
}
