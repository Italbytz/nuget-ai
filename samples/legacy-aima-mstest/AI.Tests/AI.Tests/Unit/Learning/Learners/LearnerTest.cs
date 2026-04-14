// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using AI.Tests.Unit.Learning.Framework;
using Italbytz.Adapters.Algorithms.AI.Learning.Learners;

namespace AI.Tests.Unit.Learning.Learners;

[TestClass]
public class LearnerTest
{
    [TestMethod]
    public void
        TestInducedTreeClassifiesDataSetCorrectly()
    {
        var ds = TestDataSetFactory.GetRestaurantDataSet();
        var learner = new DecisionTreeLearner();
        learner.Train(ds);
        var result = learner.Test(ds);
        Assert.AreEqual(12,result[0]);
        Assert.AreEqual(0,result[1]);
    }
}