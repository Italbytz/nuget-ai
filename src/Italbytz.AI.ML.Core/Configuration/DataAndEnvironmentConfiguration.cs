using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Italbytz.AI.ML.Core.Configuration;

public class TabularFileDataSourceV3 : MBConfig, ITabularFileDataSource
{
    public override int Version => 3;

    public override string? Type { get; set; } = "TabularFile";

    [JsonIgnore]
    public DataSourceType DataSourceType { get; set; } = DataSourceType.TabularFile;

    public IEnumerable<IColumnProperties> ColumnProperties { get; set; } = [];

    public string? FilePath { get; set; }

    public string? Delimiter { get; set; }

    public char DecimalMarker { get; set; }

    public bool HasHeader { get; set; }

    public bool AllowQuoting { get; set; }

    public char EscapeCharacter { get; set; }

    public bool ReadMultiLines { get; set; }

    [JsonIgnore]
    public bool KeepDiacritics { get; set; }

    [JsonIgnore]
    public bool KeepPunctuations { get; set; }
}

public class LocalEnvironmentV1 : MBConfig, IEnvironment
{
    public override int Version => 1;

    public override string? Type { get; set; }

    [JsonIgnore]
    public EnvironmentType EnvironmentType { get; set; }
}

public class Scenario : IScenario
{
    public ScenarioType ScenarioType { get; set; }
}

public class Parameter
{
}
