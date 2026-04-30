using Italbytz.AI.Learning;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Inductive;
using Italbytz.AI.Learning.Learners;

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

