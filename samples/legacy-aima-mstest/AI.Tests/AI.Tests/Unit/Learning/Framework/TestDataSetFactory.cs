// The original version of this file is part of <see href="https://github.com/aimacode/aima-java"/> which is released under
// MIT License
// Copyright (c) 2015 aima-java contributors

using System.Text;
using Italbytz.Adapters.Algorithms.AI.Learning.Framework;
using Italbytz.Adapters.Algorithms.AI.Util;
using Italbytz.Ports.Algorithms.AI.Learning;

namespace AI.Tests.Unit.Learning.Framework;

public class TestDataSetFactory
{
    private const string restaurant = """
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

    public static IDataSet GetCompleteRestaurantDataSet()
    {
        var spec = CreateRestaurantDataSetSpec();
        var dataString = new StringBuilder();
        IteratePossibleValues(spec, spec.GetAttributeNames().ToArray(), 0, "",
            dataString
        );
        return DataSetFactory.FromString(dataString.ToString(), spec, " ");
    }

    private static void IteratePossibleValues(IDataSetSpecification spec,
        IReadOnlyList<string> attributes, int current, string line,
        StringBuilder dataString)
    {
        if (current == attributes.Count - 1)
        {
            line += spec.GetPossibleAttributeValues(
                attributes[current]).First();
            dataString.AppendLine(line);
            return;
        }

        foreach (var value in spec.GetPossibleAttributeValues(
                     attributes[current]))
            IteratePossibleValues(spec, attributes, current + 1,
                line + $"{value} ", dataString);
    }

    public static IDataSet GetRestaurantDataSet()
    {
        var spec = CreateRestaurantDataSetSpec();
        return DataSetFactory.FromString(restaurant, spec, " ");
    }

    private static DataSetSpecification CreateRestaurantDataSetSpec()
    {
        var dss = new DataSetSpecification();
        dss.DefineStringAttribute("alternate", Util.Yesno());
        dss.DefineStringAttribute("bar", Util.Yesno());
        dss.DefineStringAttribute("fri/sat", Util.Yesno());
        dss.DefineStringAttribute("hungry", Util.Yesno());
        dss.DefineStringAttribute("patrons", [
            "None", "Some",
            "Full"
        ]);
        dss.DefineStringAttribute("price", ["$", "$$", "$$$"]);
        dss.DefineStringAttribute("raining", Util.Yesno());
        dss.DefineStringAttribute("reservation", Util.Yesno());
        dss.DefineStringAttribute("type", [
            "French", "Italian",
            "Thai", "Burger"
        ]);
        dss.DefineStringAttribute("wait_estimate", [
            "0-10",
            "10-30", "30-60", ">60"
        ]);
        dss.DefineStringAttribute("will_wait", Util.Yesno());
        return dss;
    }
}