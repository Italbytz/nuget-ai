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

public class SqlDataSourceV1 : MBConfig, IDataSource
{
    public override int Version => 1;

    public override string? Type { get; set; } = "SQL";

    [JsonIgnore]
    public DataSourceType DataSourceType { get; set; } = DataSourceType.SQL;

    public string? ConnectionString { get; set; }

    public string? CommandString { get; set; }

    public string? DatabaseName { get; set; }

    public string? SelectedTableDbo { get; set; }

    public string? TableName { get; set; }

    public IEnumerable<IColumnProperties>? ColumnProperties { get; set; }
}

public class FolderDataSourceV1 : MBConfig, IDataSource
{
    public override int Version => 1;

    public override string? Type { get; set; } = "Folder";

    [JsonIgnore]
    public DataSourceType DataSourceType { get; set; } = DataSourceType.Folder;

    public string? FolderPath { get; set; }
}

public class VottFileDataSourceV1 : MBConfig, IDataSource
{
    public override int Version => 1;

    public override string? Type { get; set; } = "Vott";

    [JsonIgnore]
    public DataSourceType DataSourceType { get; set; } = DataSourceType.Vott;

    public string? FilePath { get; set; }
}

public class CocoFileDataSourceV0 : MBConfig, IDataSource
{
    public override int Version => 0;

    public override string? Type { get; set; } = "Coco";

    [JsonIgnore]
    public DataSourceType DataSourceType { get; set; } = DataSourceType.Coco;

    public string? FilePath { get; set; }
}
