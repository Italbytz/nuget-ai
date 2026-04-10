namespace Italbytz.AI.ML.Core;

public interface IFeature
{
    string PropertyName { get; set; }

    string ColumnName { get; set; }

    int ColumnIndex { get; set; }
}
