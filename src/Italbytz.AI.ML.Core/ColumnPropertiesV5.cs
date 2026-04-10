namespace Italbytz.AI.ML.Core;

public enum ColumnDataKind
{
    String,
    Single,
    Boolean,
    DateTime,
    Int,
    StringArray
}

public enum ColumnPurposeType
{
    Label,
    Feature,
    Ignore,
    AnswerIndex
}

public class ColumnPropertiesV5
{
    public int Version { get; set; } = 5;

    public string Type { get; set; } = "Column";

    public string ColumnName { get; set; } = string.Empty;

    public ColumnPurposeType ColumnPurpose { get; set; }

    public ColumnDataKind ColumnDataFormat { get; set; }

    public bool IsCategorical { get; set; }
}
