namespace Italbytz.AI.Learning.Framework;

public static class RestaurantDataSetFactory
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

    public static IDataSet Create()
    {
        var specification = CreateSpecification();
        return DataSetFactory.FromString(Restaurant, specification, " ");
    }

    public static DataSetSpecification CreateSpecification()
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
