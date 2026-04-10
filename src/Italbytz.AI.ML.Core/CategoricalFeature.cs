namespace Italbytz.AI.ML.Core;

public class CategoricalFeature : IValueRangeFeature<string>
{
    public string PropertyName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public int ColumnIndex { get; set; }

    public List<string> ValueRange { get; set; } = [];
}
