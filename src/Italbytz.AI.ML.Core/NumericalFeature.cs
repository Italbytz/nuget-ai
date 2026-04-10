namespace Italbytz.AI.ML.Core;

public class NumericalFeature : IValueRangeFeature<float>
{
    public List<float> ValueRange { get; set; } = [];

    public string PropertyName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public int ColumnIndex { get; set; }
}
