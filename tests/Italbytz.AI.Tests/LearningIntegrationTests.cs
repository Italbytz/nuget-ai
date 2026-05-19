using Italbytz.AI.Learning;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Inductive;
using Italbytz.AI.Learning.Learners;
using System.Text;

namespace Italbytz.AI.Tests;

[TestClass]
public class LearningIntegrationTests
{
    [TestMethod]
    public void Majority_learner_classifies_restaurant_dataset_baseline()
    {
        var learner = new MajorityLearner();
        var ds = RestaurantDataSetFactory.Create();

        learner.Train(ds);
        var result = learner.Test(ds);

        CollectionAssert.AreEqual(new[] { 6, 6 }, result);
    }

    [TestMethod]
    public void Decision_tree_learner_induces_tree_that_classifies_restaurant_dataset()
    {
        var ds = RestaurantDataSetFactory.Create();
        var learner = new DecisionTreeLearner();

        learner.Train(ds);
        var result = learner.Test(ds);

        CollectionAssert.AreEqual(new[] { 12, 0 }, result);
    }

    [TestMethod]
    public void Decision_tree_stumps_are_generated_for_all_attribute_value_pairs()
    {
        var ds = RestaurantDataSetFactory.Create();

        var stumps = DecisionTree.GetStumpsFor(ds, "Yes", "Unable to classify").ToList();

        Assert.HasCount(26, stumps);
    }

    [TestMethod]
    public void Cross_validation_wrapper_selects_best_parameter_size()
    {
        var validation = new CrossValidation(0.05);
        var result = validation.CrossValidationWrapper(
            new SampleParameterizedLearner(),
            5,
            RestaurantDataSetFactory.Create());

        Assert.AreEqual(70, result.ParameterSize);
    }

    [TestMethod]
    public void Cart_decision_tree_learner_classifies_restaurant_dataset()
    {
        var ds = RestaurantDataSetFactory.Create();
        var learner = new CartDecisionTreeLearner();

        learner.Train(ds);
        var result = learner.Test(ds);

        CollectionAssert.AreEqual(new[] { 12, 0 }, result);
    }

    [TestMethod]
    public void Decision_tree_learner_matches_aima_style_generalization_on_complete_restaurant_space()
    {
        var trainingSet = RestaurantDataSetFactory.Create();
        var completeSpace = CreateCompleteRestaurantDataSet();

        var inducedLearner = new DecisionTreeLearner();
        inducedLearner.Train(trainingSet);

        var actualLearner = new DecisionTreeLearner(
            CreateActualRestaurantDecisionTree(),
            "Unable to classify");

        var inducedPredictions = inducedLearner.Predict(completeSpace);
        var actualPredictions = actualLearner.Predict(completeSpace);

        var correct = inducedPredictions
            .Zip(actualPredictions, (induced, actual) => string.Equals(induced, actual, StringComparison.Ordinal))
            .Count(match => match);
        var accuracy = (double)correct / actualPredictions.Length;

        Assert.IsTrue(
            accuracy >= 0.83 && accuracy <= 0.86,
            $"Expected AIMA-like generalization accuracy around 0.84 on complete space, but got {accuracy:F4}.");
    }

    private static IDataSet CreateCompleteRestaurantDataSet()
    {
        var spec = RestaurantDataSetFactory.CreateSpecification();
        var attributes = spec.GetAttributeNames().ToArray();
        var dataString = new StringBuilder();
        IteratePossibleValues(spec, attributes, 0, string.Empty, dataString);
        return DataSetFactory.FromString(dataString.ToString(), spec, " ");
    }

    private static void IteratePossibleValues(
        IDataSetSpecification spec,
        IReadOnlyList<string> attributes,
        int current,
        string line,
        StringBuilder dataString)
    {
        if (current == attributes.Count - 1)
        {
            line += spec.GetPossibleAttributeValues(attributes[current]).First();
            dataString.AppendLine(line);
            return;
        }

        foreach (var value in spec.GetPossibleAttributeValues(attributes[current]))
        {
            IteratePossibleValues(
                spec,
                attributes,
                current + 1,
                line + value + " ",
                dataString);
        }
    }

    private static DecisionTree CreateActualRestaurantDecisionTree()
    {
        var raining = new DecisionTree("raining");
        raining.AddLeaf("Yes", "Yes");
        raining.AddLeaf("No", "No");

        var bar = new DecisionTree("bar");
        bar.AddLeaf("Yes", "Yes");
        bar.AddLeaf("No", "No");

        var friSat = new DecisionTree("fri/sat");
        friSat.AddLeaf("Yes", "Yes");
        friSat.AddLeaf("No", "No");

        var alternate2 = new DecisionTree("alternate");
        alternate2.AddNode("Yes", raining);
        alternate2.AddLeaf("No", "Yes");

        var reservation = new DecisionTree("reservation");
        reservation.AddNode("No", bar);
        reservation.AddLeaf("Yes", "Yes");

        var alternate1 = new DecisionTree("alternate");
        alternate1.AddNode("No", reservation);
        alternate1.AddNode("Yes", friSat);

        var hungry = new DecisionTree("hungry");
        hungry.AddLeaf("No", "Yes");
        hungry.AddNode("Yes", alternate2);

        var waitEstimate = new DecisionTree("wait_estimate");
        waitEstimate.AddLeaf(">60", "No");
        waitEstimate.AddNode("30-60", alternate1);
        waitEstimate.AddNode("10-30", hungry);
        waitEstimate.AddLeaf("0-10", "Yes");

        var patrons = new DecisionTree("patrons");
        patrons.AddLeaf("None", "No");
        patrons.AddLeaf("Some", "Yes");
        patrons.AddNode("Full", waitEstimate);

        return patrons;
    }
}

internal sealed class SampleParameterizedLearner : IParameterizedLearner
{
    private bool _alpha = true;

    public int ParameterSize { get; set; }

    public void Train(IDataSet ds)
    {
    }

    public void Train(int size, IDataSet dataSet)
    {
        ParameterSize = size;
        Train(dataSet);
    }

    public string[] Predict(IDataSet ds)
    {
        throw new NotImplementedException();
    }

    public string Predict(IExample e)
    {
        throw new NotImplementedException();
    }

    public int[] Test(IDataSet ds)
    {
        var result = new int[2];
        result[0] = _alpha ? 100 : 70;
        result[1] = ParameterSize;
        _alpha = !_alpha;
        return result;
    }
}

