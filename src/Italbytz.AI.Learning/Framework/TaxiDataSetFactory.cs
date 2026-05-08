namespace Italbytz.AI.Learning.Framework;

public static class TaxiDataSetFactory
{
    // Representative subset of the NYC taxi-fare dataset.
    // Columns: passenger_count, trip_time_in_secs, trip_distance, fare_amount
    private const string Taxi = """
        1,1271,3.8,17.5
        1,474,1.5,8.0
        1,637,1.4,8.5
        1,181,0.6,4.5
        1,661,1.1,8.5
        1,935,9.6,27.5
        1,869,2.3,11.5
        1,454,1.4,7.5
        1,366,1.5,7.5
        1,252,0.6,5.0
        1,314,1.2,6.0
        1,480,0.7,7.0
        1,386,1.3,7.0
        2,351,0.8,5.5
        1,407,1.1,7.0
        2,970,5.6,19.0
        3,371,0.6,6.0
        2,367,1.3,6.5
        1,621,1.7,9.0
        1,243,0.6,4.5
        3,485,0.8,6.5
        1,88,0.4,3.5
        2,160,0.6,4.5
        2,596,1.4,8.5
        1,270,0.8,5.5
        """;

    public static IDataSet Create()
    {
        var specification = CreateSpecification();
        return DataSetFactory.FromString(Taxi, specification, ",");
    }

    public static DataSetSpecification CreateSpecification()
    {
        var specification = new DataSetSpecification();
        specification.DefineNumericAttribute("passenger_count");
        specification.DefineNumericAttribute("trip_time_in_secs");
        specification.DefineNumericAttribute("trip_distance");
        specification.DefineNumericAttribute("fare_amount");
        return specification;
    }
}
