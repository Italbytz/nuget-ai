// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using AI.Tests.Unit.Learning.Framework;
using Italbytz.Adapters.Algorithms.AI.Learning.Inductive;
using Italbytz.Adapters.Algorithms.AI.Learning.Learners;
using Italbytz.Adapters.Algorithms.AI.Util;

namespace AI.Tests.Unit.Learning.Learners;

[TestClass]
public class DecisionTreeTest
{
    [TestMethod]
    public void TestActualDecisionTreeClassifiesRestaurantDataSetCorrectly()
    {
        var learner = new DecisionTreeLearner(
            CreateActualRestaurantDecisionTree(), "Unable to classify");
        var results = learner.Test(TestDataSetFactory.GetRestaurantDataSet());
        Assert.AreEqual(12,results[0]);
        Assert.AreEqual(0,results[1]);
    }
    
    [TestMethod]
    public void TestMisclassificationOfInducedDecisionTree()
    {
        var completeRestaurantDataSet =
            TestDataSetFactory.GetCompleteRestaurantDataSet();
        var actualLearner = new DecisionTreeLearner(
            CreateActualRestaurantDecisionTree(), "Unable to classify");
        var inducedLearner = new DecisionTreeLearner(
            CreateInducedRestaurantDecisionTree(), "Unable to classify");
        var actualPredictions =
            actualLearner.Predict(completeRestaurantDataSet);
        var inducedPredictions =
            inducedLearner.Predict(completeRestaurantDataSet);
        var mcr = 0F;
        for (var i = 0; i < actualPredictions.Length; i++)
            if (!actualPredictions[i].Equals(inducedPredictions[i]))
                mcr++;
        mcr /= actualPredictions.Length;
        Assert.IsTrue(Math.Abs(mcr - 0.18) < 0.01);
    }
    
    private static DecisionTree CreateActualRestaurantDecisionTree()
    {
        // raining node
        var raining = new DecisionTree("raining");
        raining.AddLeaf(Util.Yes, Util.Yes);
        raining.AddLeaf(Util.No, Util.No);

        // bar node
        var bar = new DecisionTree("bar");
        bar.AddLeaf(Util.Yes, Util.Yes);
        bar.AddLeaf(Util.No, Util.No);

        // friday saturday node
        var frisat = new DecisionTree("fri/sat");
        frisat.AddLeaf(Util.Yes, Util.Yes);
        frisat.AddLeaf(Util.No, Util.No);

        // second alternate node to the right of the diagram below hungry
        var alternate2 = new DecisionTree("alternate");
        alternate2.AddNode(Util.Yes, raining);
        alternate2.AddLeaf(Util.No, Util.Yes);

        // reservation node
        var reservation = new DecisionTree("reservation");
        reservation.AddNode(Util.No, bar);
        reservation.AddLeaf(Util.Yes, Util.Yes);

        // first alternate node to the left of the diagram below waitestimate
        var alternate1 = new DecisionTree("alternate");
        alternate1.AddNode(Util.No, reservation);
        alternate1.AddNode(Util.Yes, frisat);

        // hungry node
        var hungry = new DecisionTree("hungry");
        hungry.AddLeaf(Util.No, Util.Yes);
        hungry.AddNode(Util.Yes, alternate2);

        // wait estimate node
        var waitEstimate = new DecisionTree("wait_estimate");
        waitEstimate.AddLeaf(">60", Util.No);
        waitEstimate.AddNode("30-60", alternate1);
        waitEstimate.AddNode("10-30", hungry);
        waitEstimate.AddLeaf("0-10", Util.Yes);

        // patrons node
        var patrons = new DecisionTree("patrons");
        patrons.AddLeaf("None", Util.No);
        patrons.AddLeaf("Some", Util.Yes);
        patrons.AddNode("Full", waitEstimate);

        return patrons;
    }
    
    private static DecisionTree CreateInducedRestaurantDecisionTree()
    {
        var frisat = new DecisionTree("fri/sat");
        frisat.AddLeaf(Util.Yes, Util.Yes);
        frisat.AddLeaf(Util.No, Util.No);

        // type node
        var type = new DecisionTree("type");
        type.AddLeaf("French", Util.Yes);
        type.AddLeaf("Italian", Util.No);
        type.AddNode("Thai", frisat);
        type.AddLeaf("Burger", Util.Yes);

        // hungry node
        var hungry = new DecisionTree("hungry");
        hungry.AddLeaf(Util.No, Util.No);
        hungry.AddNode(Util.Yes, type);

        // patrons node
        var patrons = new DecisionTree("patrons");
        patrons.AddLeaf("None", Util.No);
        patrons.AddLeaf("Some", Util.Yes);
        patrons.AddNode("Full", hungry);

        return patrons;
    }
}