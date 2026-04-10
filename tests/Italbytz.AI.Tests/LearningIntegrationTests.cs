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
        var ds = LearningTestDataSetFactory.GetRestaurantDataSet();

        learner.Train(ds);
        var result = learner.Test(ds);

        CollectionAssert.AreEqual(new[] { 6, 6 }, result);
    }

    [TestMethod]
    public void Decision_tree_learner_induces_tree_that_classifies_restaurant_dataset()
    {
        var ds = LearningTestDataSetFactory.GetRestaurantDataSet();
        var learner = new DecisionTreeLearner();

        learner.Train(ds);
        var result = learner.Test(ds);

        CollectionAssert.AreEqual(new[] { 12, 0 }, result);
    }

    [TestMethod]
    public void Decision_tree_stumps_are_generated_for_all_attribute_value_pairs()
    {
        var ds = LearningTestDataSetFactory.GetRestaurantDataSet();

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
            LearningTestDataSetFactory.GetRestaurantDataSet());

        Assert.AreEqual(70, result.ParameterSize);
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

internal static class LearningTestDataSetFactory
{
    private const string Restaurant = """
                                      Yes No  No  Yes Some $$$ No   Yes French  0-10   Yes
                                      Yes No  No  Yes Full $   No   No  Thai    30-60  No
                                      No  Yes No  No  Some $   No   No  Burger  0-10   Yes
                                      Yes No  Yes Yes Full $   Yes   No  Thai    10-30  Yes
                                      Yes No  Yes No  Full $$$ No   Yes French  >60    No
                                      No  Yes No  Yes Some $$  Yes  Yes Italian 0-10   Yes
                                      No  Yes No  No  None $   Yes  No  Burger  0-10   No
                                      No  No  No  Yes Some $$  Yes  Yes Thai    0-10   Yes
                                      No  Yes Yes No  Full $   Yes  No  Burger  >60    No
                                      Yes Yes Yes Yes Full $$$ No   Yes Italian 10-30  No
                                      No  No  No  No  None $   No   No  Thai    0-10   No
                                      Yes Yes Yes Yes Full $   No   No  Burger  30-60  Yes
                                      """;

    public static IDataSet GetRestaurantDataSet()
    {
        var specification = CreateRestaurantDataSetSpecification();
        return DataSetFactory.FromString(Restaurant, specification, " ");
    }

    private static DataSetSpecification CreateRestaurantDataSetSpecification()
    {
        var specification = new DataSetSpecification();
        specification.DefineStringAttribute("alternate", ["Yes", "No"]);
        specification.DefineStringAttribute("bar", ["Yes", "No"]);
        specification.DefineStringAttribute("fri/sat", ["Yes", "No"]);
        specification.DefineStringAttribute("hungry", ["Yes", "No"]);
        specification.DefineStringAttribute("patrons", ["None", "Some", "Full"]);
        specification.DefineStringAttribute("price", ["$", "$$", "$$$"]);
        specification.DefineStringAttribute("raining", ["Yes", "No"]);
        specification.DefineStringAttribute("reservation", ["Yes", "No"]);
        specification.DefineStringAttribute("type", ["French", "Italian", "Thai", "Burger"]);
        specification.DefineStringAttribute("wait_estimate", ["0-10", "10-30", "30-60", ">60"]);
        specification.DefineStringAttribute("will_wait", ["Yes", "No"]);
        return specification;
    }
}
