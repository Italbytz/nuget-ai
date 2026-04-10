using System.Text.Json;
using System.Text.Json.Serialization;

namespace Italbytz.AI.ML.Core.Configuration;

public class TrainingConfiguration : MBConfig, ITrainingConfiguration
{
    public override int Version => 5;

    public override string? Type { get; set; } = "TrainingConfig";

    public ScenarioType Scenario { get; set; }

    public IDataSource? DataSource { get; set; }

    public IEnvironment? Environment { get; set; }

    [JsonIgnore]
    public AutoMLType? AutoMLType { get; set; } = Configuration.AutoMLType.Octopus;

    [JsonIgnore]
    public ITrainResult? TrainResult { get; set; }

    public ITrainingOption? TrainingOption { get; set; }

    [JsonIgnore]
    public string? TrainingConfigurationFolder { get; set; }

    public string SerializeToJson(bool writeIndented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Serialize(this, options);
    }
}
