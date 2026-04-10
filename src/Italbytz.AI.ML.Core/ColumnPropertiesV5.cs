using Italbytz.AI.ML.Core.Configuration;

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
    User,
    Item,
    Text,
    SourceSentence,
    ComparisonSentence,
    Sentence,
    Context,
    Question,
    AnswerIndex,
    Feature,
    Ignore
}

public class ColumnPropertiesV5 : IColumnProperties
{
    public int Version { get; set; } = 5;

    public string Type { get; set; } = "Column";

    public string? ColumnName { get; set; } = string.Empty;

    public ColumnPurposeType ColumnPurpose { get; set; }

    public ColumnDataKind ColumnDataFormat { get; set; }

    public bool IsCategorical { get; set; }
}
